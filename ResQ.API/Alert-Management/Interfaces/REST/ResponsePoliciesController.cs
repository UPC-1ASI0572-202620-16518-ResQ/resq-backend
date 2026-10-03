using Microsoft.AspNetCore.Mvc;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;
using ResQ.API.Alert_Management.Interfaces.REST.Transform;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Alert_Management.Interfaces.REST;

/// <summary>
/// Controlador REST de las políticas de respuesta por tipo de riesgo (Alert Management Bounded Context).
/// </summary>
[ApiController]
[Route("api/v1/response-policies")]
[Produces("application/json")]
[Authorize]
public class ResponsePoliciesController(
    IResponsePolicyCommandService responsePolicyCommandService,
    IResponsePolicyQueryService responsePolicyQueryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar políticas de respuesta",
        Description = "Obtiene las políticas de respuesta de la organización. Filtro opcional `riskTypeCode`.",
        OperationId = "GetResponsePolicies")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de políticas", typeof(IEnumerable<ResponsePolicyResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Filtro inválido")]
    public async Task<IActionResult> GetResponsePolicies([FromQuery] string? riskTypeCode)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var policies = await responsePolicyQueryService.Handle(new GetResponsePoliciesQuery(organizationId, riskTypeCode));

            return Ok(policies.Select(ResponsePolicyResourceFromEntityAssembler.ToResourceFromEntity));
        });
    }

    [HttpGet("{policyId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener política de respuesta",
        Description = "Obtiene una política de respuesta con sus acciones.",
        OperationId = "GetResponsePolicyById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Política encontrada", typeof(ResponsePolicyResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Política no encontrada")]
    public async Task<IActionResult> GetResponsePolicyById(Guid policyId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var policy = await responsePolicyQueryService.Handle(new GetResponsePolicyByIdQuery(organizationId, policyId));

        if (policy is null) return NotFound(new { message = "Response policy not found." });

        return Ok(ResponsePolicyResourceFromEntityAssembler.ToResourceFromEntity(policy));
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Configurar política de respuesta",
        Description = @"**Propósito:**
Crea una política de respuesta para un tipo de riesgo. La política nace **INACTIVE** con `version` 1.

**Integración con Device Management:** cada acción se valida con `IDevicesContextFacade`: el dispositivo debe existir,
no estar retirado y tener la capacidad indicada como capacidad de **actuación**.

---
### JSON de Prueba
*Reemplaza `targetDeviceId` por un dispositivo real con una capacidad de actuación (p. ej. `ALARM_BUZZER`).*
```json
{
  ""riskTypeCode"": ""GAS_LEAK"",
  ""actions"": [
    {
      ""actionCode"": ""ACTIVATE_AUDIBLE_ALARM"",
      ""targetDeviceId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
      ""targetCapabilityCode"": ""ALARM_BUZZER"",
      ""authorizationMode"": ""AUTOMATIC"",
      ""critical"": true
    },
    {
      ""actionCode"": ""CUT_GAS_VALVE"",
      ""targetDeviceId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
      ""targetCapabilityCode"": ""GAS_VALVE"",
      ""authorizationMode"": ""HUMAN_REQUIRED"",
      ""critical"": true
    }
  ]
}
```",
        OperationId = "ConfigureResponsePolicy")]
    [SwaggerResponse(StatusCodes.Status201Created, "Política creada", typeof(ResponsePolicyResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Dispositivo o capacidad de destino inválidos")]
    public async Task<IActionResult> ConfigureResponsePolicy([FromBody] ConfigureResponsePolicyResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = ResponsePolicyCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);
            var policy = await responsePolicyCommandService.Handle(command);

            if (policy is null) return BadRequest();

            var policyResource = ResponsePolicyResourceFromEntityAssembler.ToResourceFromEntity(policy);
            return CreatedAtAction(nameof(GetResponsePolicyById), new { policyId = policy.Id }, policyResource);
        });
    }

    [HttpPut("{policyId:guid}")]
    [SwaggerOperation(
        Summary = "Actualizar política de respuesta",
        Description = @"**Propósito:**
Reemplaza el tipo de riesgo y las acciones de la política e incrementa `version`.
Las acciones conservan su `actionId` si mantienen el mismo `actionCode`.
Una política **ACTIVE** debe conservar al menos una acción. Mismo cuerpo que la creación.",
        OperationId = "UpdateResponsePolicy")]
    [SwaggerResponse(StatusCodes.Status200OK, "Política actualizada", typeof(ResponsePolicyResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Política no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Regla de negocio incumplida")]
    public async Task<IActionResult> UpdateResponsePolicy(Guid policyId, [FromBody] UpdateResponsePolicyResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = ResponsePolicyCommandFromResourceAssembler.ToCommandFromResource(organizationId, policyId, resource);
            var policy = await responsePolicyCommandService.Handle(command);

            if (policy is null) return NotFound(new { message = "Response policy not found." });

            return Ok(ResponsePolicyResourceFromEntityAssembler.ToResourceFromEntity(policy));
        });
    }

    [HttpPut("{policyId:guid}/status")]
    [SwaggerOperation(
        Summary = "Activar o desactivar política",
        Description = @"**Propósito:**
Activa o desactiva la política. Para activarla debe tener al menos una acción y no puede haber
otra política **ACTIVE** para el mismo tipo de riesgo.

---
### JSON de Prueba
```json
{
  ""active"": true
}
```",
        OperationId = "ChangeResponsePolicyStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado actualizado", typeof(ResponsePolicyResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Política no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Sin acciones u otra política activa para el mismo riesgo")]
    public async Task<IActionResult> ChangeResponsePolicyStatus(Guid policyId, [FromBody] ChangeResponsePolicyStatusResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = ResponsePolicyCommandFromResourceAssembler.ToCommandFromResource(organizationId, policyId, resource);
            var policy = await responsePolicyCommandService.Handle(command);

            if (policy is null) return NotFound(new { message = "Response policy not found." });

            return Ok(ResponsePolicyResourceFromEntityAssembler.ToResourceFromEntity(policy));
        });
    }
}
