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
    IAlertQueryService alertQueryService) : ControllerBase
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
Genera una alerta a partir de una detección de riesgo y solicita una notificación por destinatario.
Si existe una política de respuesta **ACTIVE** para el `riskTypeCode`, se crean sus ejecuciones de respuesta:
las acciones `AUTOMATIC` quedan en `EXECUTION_REQUESTED` y las `HUMAN_REQUIRED` en `PENDING_AUTHORIZATION`.

Mientras no exista el contexto Risk Detection, este endpoint es el punto de entrada para generar alertas
(Risk Detection usará después `IAlertsContextFacade`).

**Integración con Building Management:** si se envía `buildingId` (y opcionalmente `zoneId`), se valida que existan y estén activos.

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
    [SwaggerResponse(StatusCodes.Status409Conflict, "Edificación o zona inexistente o inactiva")]
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

    [HttpPut("{alertId:guid}/deliveries/{deliveryId:guid}/status")]
    [SwaggerOperation(
        Summary = "Registrar resultado de notificación",
        Description = @"**Propósito:**
Registra el resultado informado por el proveedor de notificaciones para una entrega `PENDING`.

---
### JSON de Prueba
```json
{
  ""status"": ""FAILED"",
  ""failureReason"": ""The push provider did not confirm delivery.""
}
```
`status` admite `DELIVERED` o `FAILED`; `failureReason` es obligatorio cuando falla.",
        OperationId = "RecordNotificationDeliveryOutcome")]
    [SwaggerResponse(StatusCodes.Status200OK, "Alerta actualizada", typeof(AlertResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Estado o motivo inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Alerta o entrega no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La entrega ya tiene un resultado registrado")]
    public async Task<IActionResult> RecordNotificationDeliveryOutcome(Guid alertId, Guid deliveryId,
        [FromBody] RecordNotificationDeliveryOutcomeResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = RecordNotificationDeliveryOutcomeCommandFromResourceAssembler
                .ToCommandFromResource(organizationId, alertId, deliveryId, resource);
            var alert = await alertCommandService.Handle(command);

            if (alert is null) return NotFound(new { message = "Alert not found." });

            return Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(alert));
        });
    }
}
