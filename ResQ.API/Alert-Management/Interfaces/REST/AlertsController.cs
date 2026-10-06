using Microsoft.AspNetCore.Mvc;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;
using ResQ.API.Alert_Management.Interfaces.REST.Transform;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Alert_Management.Interfaces.REST;

/// <summary>
/// Controlador REST de alertas tempranas (Alert Management Bounded Context).
/// Una alerta avisa de un riesgo detectado antes de que ocurra la emergencia; la emergencia en curso
/// se gestiona como incidente en Incident Management.
/// </summary>
[ApiController]
[Route("api/v1/alerts")]
[Produces("application/json")]
[Authorize]
public class AlertsController(
    IAlertCommandService alertCommandService,
    IAlertQueryService alertQueryService,
    IResponseExecutionCommandService responseExecutionCommandService,
    IResponseExecutionQueryService responseExecutionQueryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar alertas",
        Description = @"**Propósito:**
Obtiene las alertas de la organización, de la más reciente a la más antigua.

---
### Filtros opcionales (query)
* `buildingId`: GUID de la edificación.
* `zoneId`: GUID de la zona.
* `riskTypeCode`: código de riesgo (p. ej. `GAS_LEAK`, `FIRE`).
* `from` / `to`: rango ISO-8601 sobre `generatedAt`.",
        OperationId = "GetAlerts")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de alertas", typeof(IEnumerable<AlertResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Filtros inválidos")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] Guid? buildingId,
        [FromQuery] Guid? zoneId,
        [FromQuery] string? riskTypeCode,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (from.HasValue && to.HasValue && from > to)
            return BadRequest(new { message = "'from' must be earlier than 'to'." });

        return await this.ExecuteAsync(async () =>
        {
            var query = new GetAlertsQuery(organizationId, buildingId, zoneId, riskTypeCode, from, to);
            var alerts = await alertQueryService.Handle(query);

            return Ok(alerts.Select(AlertResourceFromEntityAssembler.ToResourceFromEntity));
        });
    }

    [HttpGet("{alertId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener alerta",
        Description = "Obtiene una alerta con su contexto de detección y el estado de sus notificaciones.",
        OperationId = "GetAlertById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Alerta encontrada", typeof(AlertResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Alerta no encontrada")]
    public async Task<IActionResult> GetAlertById(Guid alertId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var alert = await alertQueryService.Handle(new GetAlertByIdQuery(organizationId, alertId));

        if (alert is null) return NotFound(new { message = "Alert not found." });

        return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Generar alerta",
        Description = @"**Propósito:**
Genera una alerta a partir de una detección de riesgo, solicita una notificación por destinatario y,
si se envían `responseActions`, crea una ejecución de respuesta por acción:
las `AUTOMATIC` quedan en `EXECUTION_REQUESTED` y las `HUMAN_REQUIRED` en `PENDING_AUTHORIZATION`.

Mientras no exista el contexto Risk Detection, este endpoint es el punto de entrada para generar alertas
(Risk Detection usará después `IAlertsContextFacade`).

**Integración con Building Management:** si se envía `buildingId` (y opcionalmente `zoneId`), se valida que existan y estén activos.

**Integración con Device Management:** cada `targetDeviceId` debe ser un dispositivo activo con una capacidad de actuación `targetCapabilityCode`.

---
### Encabezados Requeridos (Headers)
* Authorization: Bearer {token_jwt}
* Content-Type: application/json

---
### JSON de Prueba
```json
{
  ""riskDetectionId"": ""RISK-0001"",
  ""riskTypeCode"": ""GAS_LEAK"",
  ""severityCode"": ""Critical"",
  ""buildingId"": null,
  ""zoneId"": null,
  ""detectedAt"": ""2026-10-03T15:00:00Z"",
  ""recipients"": [
    {
      ""recipientUserId"": ""1"",
      ""channel"": ""PUSH"",
      ""destination"": ""registered-mobile-device""
    }
  ],
  ""responseActions"": [
    {
      ""actionCode"": ""CLOSE_GAS_VALVE"",
      ""targetDeviceId"": ""{deviceId}"",
      ""targetCapabilityCode"": ""gas-valve"",
      ""authorizationMode"": ""HUMAN_REQUIRED"",
      ""critical"": true
    }
  ]
}
```

---
Severidades soportadas: `Info`, `Warning`, `Critical`.

### Respuesta Exitosa (201 Created)
Retorna el AlertResource con las notificaciones en estado `PENDING`.",
        OperationId = "GenerateAlert")]
    [SwaggerResponse(StatusCodes.Status201Created, "Alerta generada", typeof(AlertResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Edificación, zona o dispositivo inexistente o inactivo")]
    public async Task<IActionResult> GenerateAlert([FromBody] GenerateAlertResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = GenerateAlertCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);
            var alert = await alertCommandService.Handle(command);

            if (alert is null) return BadRequest();

            var alertResource = AlertResourceFromEntityAssembler.ToResourceFromEntity(alert);
            return CreatedAtAction(nameof(GetAlertById), new { alertId = alert.Id }, alertResource);
        });
    }

    [HttpGet("{alertId:guid}/response-executions")]
    [SwaggerOperation(
        Summary = "Listar ejecuciones de respuesta de una alerta",
        Description = @"**Propósito:**
Obtiene las acciones de respuesta solicitadas para la alerta (p. ej. cerrar una válvula de gas), en el orden en que se pidieron.

Estados: `EXECUTION_REQUESTED` (automática), `PENDING_AUTHORIZATION` (requiere decisión humana), `AUTHORIZED` o `REJECTED`.",
        OperationId = "GetResponseExecutionsByAlertId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ejecuciones de la alerta", typeof(IEnumerable<ResponseExecutionResource>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Alerta no encontrada")]
    public async Task<IActionResult> GetResponseExecutionsByAlertId(Guid alertId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var alert = await alertQueryService.Handle(new GetAlertByIdQuery(organizationId, alertId));

        if (alert is null) return NotFound(new { message = "Alert not found." });

        var executions = await responseExecutionQueryService.Handle(new GetResponseExecutionsByAlertIdQuery(organizationId, alertId));

        return Ok(executions.Select(ResponseExecutionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPut("{alertId:guid}/response-executions/{responseExecutionId:guid}/authorization")]
    [SwaggerOperation(
        Summary = "Autorizar o rechazar una ejecución de respuesta",
        Description = @"**Propósito:**
Registra la decisión humana sobre una ejecución `HUMAN_REQUIRED` que está en `PENDING_AUTHORIZATION`.
El usuario que decide se toma del token.

---
### JSON de Prueba
```json
{
  ""decision"": ""APPROVED""
}
```
`decision` admite `APPROVED` o `REJECTED`.",
        OperationId = "DecideResponseAuthorization")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ejecución actualizada", typeof(ResponseExecutionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Decisión inválida")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Alerta o ejecución no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La ejecución no requiere autorización o ya fue decidida")]
    public async Task<IActionResult> DecideResponseAuthorization(Guid alertId, Guid responseExecutionId,
        [FromBody] DecideResponseAuthorizationResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var userId = HttpContext.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = DecideResponseAuthorizationCommandFromResourceAssembler
                .ToCommandFromResource(organizationId, alertId, responseExecutionId, userId, resource);
            var execution = await responseExecutionCommandService.Handle(command);

            if (execution is null) return NotFound(new { message = "Response execution not found." });

            return Ok(ResponseExecutionResourceFromEntityAssembler.ToResourceFromEntity(execution));
        });
    }
}
