using Microsoft.AspNetCore.Mvc;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Risk_Detection.Domain.Model.Queries;
using ResQ.API.Risk_Detection.Domain.Services;
using ResQ.API.Risk_Detection.Interfaces.REST.Resources;
using ResQ.API.Risk_Detection.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Risk_Detection.Interfaces.REST;

/// <summary>
/// Controlador REST de detecciones de riesgo (Risk Detection Bounded Context).
/// Expone las detecciones persistidas con su evidencia y la evaluación de mediciones.
/// </summary>
[ApiController]
[Route("api/v1/risk-detections")]
[Produces("application/json")]
[Authorize]
public class RiskDetectionsController(
    IRiskDetectionCommandService riskDetectionCommandService,
    IRiskDetectionQueryService riskDetectionQueryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar detecciones de riesgo",
        Description = @"Detecciones de la organización, de la más reciente a la más antigua.

### Filtros opcionales (query)
* `buildingId`, `zoneId`: GUID.
* `riskTypeCode`: p. ej. `GAS_LEAK`.",
        OperationId = "GetRiskDetections")]
    [SwaggerResponse(StatusCodes.Status200OK, "Detecciones", typeof(IEnumerable<RiskDetectionResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Filtros inválidos")]
    public async Task<IActionResult> GetRiskDetections([FromQuery] Guid? buildingId, [FromQuery] Guid? zoneId,
        [FromQuery] string? riskTypeCode)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var detections = await riskDetectionQueryService.Handle(
                new GetRiskDetectionsQuery(organizationId, buildingId, zoneId, riskTypeCode));

            return Ok(detections.Select(RiskDetectionResourceFromEntityAssembler.ToResourceFromEntity));
        });
    }

    [HttpGet("{riskDetectionId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener detección de riesgo",
        Description = "Tipo de riesgo, severidad actual, ubicación (`RESOLVED`/`UNRESOLVED`), evidencia y cambios de severidad.",
        OperationId = "GetRiskDetectionById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Detección encontrada", typeof(RiskDetectionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Detección no encontrada")]
    public async Task<IActionResult> GetRiskDetectionById(Guid riskDetectionId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var detection = await riskDetectionQueryService.Handle(new GetRiskDetectionQuery(organizationId, riskDetectionId));

        if (detection is null) return NotFound(new { message = "Risk detection not found." });

        return Ok(RiskDetectionResourceFromEntityAssembler.ToResourceFromEntity(detection));
    }

    [HttpGet("{riskDetectionId:guid}/evidence")]
    [SwaggerOperation(
        Summary = "Obtener evidencia de una detección",
        Description = @"Devuelve la detección con su evidencia (medición, dispositivo, variable, valor y momento)
y la trazabilidad de severidad. Mantiene la misma forma que `RiskDetectionResource`, como espera el frontend.",
        OperationId = "GetRiskDetectionEvidence")]
    [SwaggerResponse(StatusCodes.Status200OK, "Evidencia de la detección", typeof(RiskDetectionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Detección no encontrada")]
    public async Task<IActionResult> GetRiskDetectionEvidence(Guid riskDetectionId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var detection = await riskDetectionQueryService.Handle(new GetRiskDetectionEvidenceQuery(organizationId, riskDetectionId));

        if (detection is null) return NotFound(new { message = "Risk detection not found." });

        return Ok(RiskDetectionResourceFromEntityAssembler.ToResourceFromEntity(detection));
    }

    [HttpPost("evaluations")]
    [SwaggerOperation(
        Summary = "Evaluar medición",
        Description = @"**Propósito:**
Evalúa una medición válida contra las reglas `ACTIVE` de su `variableType`. Mientras no exista Monitoring,
este endpoint es el punto de entrada (Monitoring usará `IRiskDetectionContextFacade.EvaluateMeasurementAsync`).

* Por cada tipo de riesgo se aplica la regla satisfecha más severa.
* La ubicación se resuelve con Device Management (`RESOLVED`); si el dispositivo no existe queda `UNRESOLVED`.
* Si ya hay una detección del mismo tipo y ubicación con evidencia dentro de `RiskDetectionSettings:CorrelationWindowMinutes`,
  se añade la evidencia y, si cambia la severidad, se registra un `severityChange`.
* Una detección nueva o una severidad que sube generan una alerta en Alert & Response (`generatedAlertIds`).

---
### JSON de Prueba
```json
{
  ""deviceId"": ""{deviceId}"",
  ""variableType"": ""GAS_PPM"",
  ""value"": 750,
  ""measuredAt"": ""2026-11-11T15:00:00Z"",
  ""measurementId"": null
}
```",
        OperationId = "EvaluateMeasurement")]
    [SwaggerResponse(StatusCodes.Status200OK, "Resultado de la evaluación", typeof(MeasurementEvaluationResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Medición inválida")]
    public async Task<IActionResult> EvaluateMeasurement([FromBody] EvaluateMeasurementResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = RiskDetectionCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);
            var result = await riskDetectionCommandService.Handle(command);

            return Ok(RiskDetectionResourceFromEntityAssembler.ToResourceFromResult(result));
        });
    }
}
