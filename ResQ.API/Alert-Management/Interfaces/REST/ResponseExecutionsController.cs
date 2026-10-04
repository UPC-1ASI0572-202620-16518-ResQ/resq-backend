using Microsoft.AspNetCore.Mvc;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;
using ResQ.API.Alert_Management.Interfaces.REST.Transform;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Alert_Management.Interfaces.REST;

/// <summary>
/// Controlador REST de las ejecuciones de respuesta solicitadas por las alertas (Alert Management Bounded Context).
/// </summary>
[ApiController]
[Route("api/v1/response-executions")]
[Produces("application/json")]
[Authorize]
public class ResponseExecutionsController(
    IResponseExecutionCommandService responseExecutionCommandService,
    IResponseExecutionQueryService responseExecutionQueryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar ejecuciones de respuesta",
        Description = @"**Propósito:**
Obtiene las ejecuciones de respuesta de la organización, de la más reciente a la más antigua.

---
### Filtros opcionales (query)
* `riskDetectionId`: detección de riesgo que originó la alerta.
* `alertId`: GUID de la alerta.
* `status`: `PENDING`, `PENDING_AUTHORIZATION`, `AUTHORIZED`, `EXECUTION_REQUESTED`, `SUCCEEDED`, `FAILED` o `REJECTED`.
* `from` / `to`: rango ISO-8601 sobre `requestedAt`.",
        OperationId = "GetResponseExecutions")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de ejecuciones", typeof(IEnumerable<ResponseExecutionResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Filtros inválidos")]
    public async Task<IActionResult> GetResponseExecutions(
        [FromQuery] string? riskDetectionId,
        [FromQuery] Guid? alertId,
        [FromQuery] string? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (from.HasValue && to.HasValue && from > to)
            return BadRequest(new { message = "'from' must be earlier than 'to'." });

        return await this.ExecuteAsync(async () =>
        {
            EResponseExecutionStatus? parsedStatus = string.IsNullOrWhiteSpace(status)
                ? null
                : EnumCode.Parse<EResponseExecutionStatus>(status, "status");

            var query = new GetResponseExecutionsQuery(organizationId, riskDetectionId, alertId, parsedStatus, from, to);
            var executions = await responseExecutionQueryService.Handle(query);

            return Ok(executions.Select(ResponseExecutionResourceFromEntityAssembler.ToResourceFromEntity));
        });
    }

    [HttpGet("{responseExecutionId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener ejecución de respuesta",
        Description = "Obtiene una ejecución de respuesta con la acción, la autorización y el resultado registrados.",
        OperationId = "GetResponseExecutionById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ejecución encontrada", typeof(ResponseExecutionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ejecución no encontrada")]
    public async Task<IActionResult> GetResponseExecutionById(Guid responseExecutionId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var execution = await responseExecutionQueryService.Handle(
            new GetResponseExecutionByIdQuery(organizationId, responseExecutionId));

        if (execution is null) return NotFound(new { message = "Response execution not found." });

        return Ok(ResponseExecutionResourceFromEntityAssembler.ToResourceFromEntity(execution));
    }

    [HttpPost("{responseExecutionId:guid}/authorization")]
    [SwaggerOperation(
        Summary = "Decidir autorización humana",
        Description = @"**Propósito:**
Registra la decisión del operador autenticado para una ejecución `HUMAN_REQUIRED` que está en `PENDING_AUTHORIZATION`.
`APPROVED` la pasa a `AUTHORIZED`; `REJECTED` la pasa a `REJECTED`. Solo se admite una decisión.

---
### JSON de Prueba
```json
{
  ""decision"": ""APPROVED""
}
```",
        OperationId = "DecideResponseAuthorization")]
    [SwaggerResponse(StatusCodes.Status200OK, "Decisión registrada", typeof(ResponseExecutionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Decisión inválida")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ejecución no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "No requiere autorización, no está pendiente o ya fue decidida")]
    public async Task<IActionResult> DecideResponseAuthorization(Guid responseExecutionId,
        [FromBody] DecideResponseAuthorizationResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var userId = HttpContext.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = DecideResponseAuthorizationCommandFromResourceAssembler
                .ToCommandFromResource(organizationId, responseExecutionId, userId, resource);
            var execution = await responseExecutionCommandService.Handle(command);

            if (execution is null) return NotFound(new { message = "Response execution not found." });

            return Ok(ResponseExecutionResourceFromEntityAssembler.ToResourceFromEntity(execution));
        });
    }

    [HttpPut("{responseExecutionId:guid}/result")]
    [SwaggerOperation(
        Summary = "Registrar resultado del actuador",
        Description = @"**Propósito:**
Registra el resultado reportado por el actuador para una ejecución en `EXECUTION_REQUESTED` o `AUTHORIZED`.
La ejecución pasa a `SUCCEEDED` o `FAILED` según `successful`.

---
### JSON de Prueba
```json
{
  ""successful"": true,
  ""resultCode"": ""ACTUATOR_CONFIRMED"",
  ""message"": ""The audible alarm was activated locally.""
}
```",
        OperationId = "RecordResponseExecutionResult")]
    [SwaggerResponse(StatusCodes.Status200OK, "Resultado registrado", typeof(ResponseExecutionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Ejecución no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La ejecución no admite un resultado en su estado actual")]
    public async Task<IActionResult> RecordResponseExecutionResult(Guid responseExecutionId,
        [FromBody] RecordResponseExecutionResultResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = RecordResponseExecutionResultCommandFromResourceAssembler
                .ToCommandFromResource(organizationId, responseExecutionId, resource);
            var execution = await responseExecutionCommandService.Handle(command);

            if (execution is null) return NotFound(new { message = "Response execution not found." });

            return Ok(ResponseExecutionResourceFromEntityAssembler.ToResourceFromEntity(execution));
        });
    }
}
