using Microsoft.AspNetCore.Mvc;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Risk_Detection.Domain.Model.Queries;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Services;
using ResQ.API.Risk_Detection.Interfaces.REST.Resources;
using ResQ.API.Risk_Detection.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Risk_Detection.Interfaces.REST;

/// <summary>
/// Controlador REST de reglas de detección (Risk Detection Bounded Context).
/// Una regla define la condición sobre una variable monitoreada que produce un riesgo de cierto tipo y severidad.
/// </summary>
[ApiController]
[Route("api/v1/risk-detection/rules")]
[Produces("application/json")]
[Authorize]
public class DetectionRulesController(
    IDetectionRuleCommandService detectionRuleCommandService,
    IDetectionRuleQueryService detectionRuleQueryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Configurar regla de detección",
        Description = @"**Propósito:**
Crea una regla de detección. La regla nace `ACTIVE` y participa en la evaluación de las mediciones de su `variableType`.

---
### JSON de Prueba
```json
{
  ""riskTypeCode"": ""GAS_LEAK"",
  ""severityCode"": ""Critical"",
  ""variableType"": ""GAS_PPM"",
  ""operator"": ""GREATER_THAN"",
  ""threshold"": 600
}
```
* `operator`: `GREATER_THAN`, `GREATER_THAN_OR_EQUAL`, `LESS_THAN`, `LESS_THAN_OR_EQUAL`, `EQUAL`.
* `severityCode`: `Info`, `Warning`, `Critical` (el catálogo de Alert & Response).
* `riskTypeCode` y `variableType` en UPPER_SNAKE_CASE (se normalizan).",
        OperationId = "ConfigureDetectionRule")]
    [SwaggerResponse(StatusCodes.Status201Created, "Regla creada", typeof(DetectionRuleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    public async Task<IActionResult> ConfigureDetectionRule([FromBody] ConfigureDetectionRuleResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = RiskDetectionCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);
            var rule = await detectionRuleCommandService.Handle(command);

            return CreatedAtAction(nameof(GetDetectionRuleById), new { ruleId = rule.Id },
                DetectionRuleResourceFromEntityAssembler.ToResourceFromEntity(rule));
        });
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar reglas de detección",
        Description = @"Lista las reglas de la organización en orden de configuración.

### Filtros opcionales (query)
* `status`: `ACTIVE` o `INACTIVE`.
* `variableType`: p. ej. `GAS_PPM`.",
        OperationId = "GetDetectionRules")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reglas", typeof(IEnumerable<DetectionRuleResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Filtros inválidos")]
    public async Task<IActionResult> GetDetectionRules([FromQuery] string? status, [FromQuery] string? variableType)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            EDetectionRuleStatus? statusFilter = string.IsNullOrWhiteSpace(status)
                ? null
                : EnumCode.Parse<EDetectionRuleStatus>(status, "status");

            var rules = await detectionRuleQueryService.Handle(new GetDetectionRulesQuery(organizationId, statusFilter, variableType));

            return Ok(rules.Select(DetectionRuleResourceFromEntityAssembler.ToResourceFromEntity));
        });
    }

    [HttpGet("{ruleId:guid}")]
    [SwaggerOperation(Summary = "Obtener regla de detección", OperationId = "GetDetectionRuleById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Regla encontrada", typeof(DetectionRuleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Regla no encontrada")]
    public async Task<IActionResult> GetDetectionRuleById(Guid ruleId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var rule = await detectionRuleQueryService.Handle(new GetDetectionRuleByIdQuery(organizationId, ruleId));

        if (rule is null) return NotFound(new { message = "Detection rule not found." });

        return Ok(DetectionRuleResourceFromEntityAssembler.ToResourceFromEntity(rule));
    }

    [HttpPatch("{ruleId:guid}/status")]
    [SwaggerOperation(
        Summary = "Activar o desactivar regla de detección",
        Description = @"Una regla `INACTIVE` no participa en la evaluación de mediciones.

### JSON de Prueba
```json
{
  ""active"": false
}
```",
        OperationId = "ChangeDetectionRuleStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Regla actualizada", typeof(DetectionRuleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Falta 'active'")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Regla no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La regla tiene una condición inválida y no puede activarse")]
    public async Task<IActionResult> ChangeDetectionRuleStatus(Guid ruleId, [FromBody] ChangeDetectionRuleStatusResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = RiskDetectionCommandFromResourceAssembler.ToCommandFromResource(organizationId, ruleId, resource);
            var rule = await detectionRuleCommandService.Handle(command);

            if (rule is null) return NotFound(new { message = "Detection rule not found." });

            return Ok(DetectionRuleResourceFromEntityAssembler.ToResourceFromEntity(rule));
        });
    }
}
