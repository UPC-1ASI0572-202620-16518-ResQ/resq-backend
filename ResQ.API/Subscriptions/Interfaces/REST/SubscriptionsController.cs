using Microsoft.AspNetCore.Mvc;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Subscriptions.Domain.Model.Commands;
using ResQ.API.Subscriptions.Domain.Model.Queries;
using ResQ.API.Subscriptions.Domain.Services;
using ResQ.API.Subscriptions.Interfaces.REST.Resources;
using ResQ.API.Subscriptions.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Subscriptions.Interfaces.REST;

/// <summary>
/// Controlador REST para la gestión de suscripciones
/// (Subscriptions Bounded Context).
/// </summary>
[ApiController]
[Route("api/v1/subscriptions")]
[Produces("application/json")]
[Authorize]
public class SubscriptionsController(
    ISubscriptionCommandService subscriptionCommandService,
    ISubscriptionQueryService subscriptionQueryService) : ControllerBase
{
    /// <summary>
    /// Crea una nueva suscripción para la organización autenticada.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Crear suscripción",
        Description = @"**Propósito:**
Registra una nueva suscripción para la organización autenticada.

La organización se obtiene del contexto de autenticación y no debe enviarse
en el payload.

La organización solo puede tener una suscripción activa o registrada
dentro del contexto.

---
### Encabezados Requeridos

* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO

```json
{
  ""startDate"": ""2026-10-03T00:00:00"",
  ""endDate"": ""2027-10-03T00:00:00""
}
```

---
Respuesta Exitosa

Retorna la suscripción creada con código 201 Created.",
        OperationId = "CreateSubscription")]
    [SwaggerResponse(StatusCodes.Status201Created, "Suscripción creada exitosamente", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Organización no encontrada en el contexto de autenticación")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La organización ya posee una suscripción")]
    
    public async Task<IActionResult> CreateSubscription(
        [FromBody] CreateSubscriptionResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();
        try
        {
            var command = CreateSubscriptionCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);

            var subscription = await subscriptionCommandService.Handle(command);

            if (subscription == null)
                return Conflict(new
                {
                    message = "The organization already has a subscription."
                });

            var subscriptionResource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);

            return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id.Value }, subscriptionResource);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }
    
    
    /// <summary>
    /// Obtiene la suscripción de la organización autenticada.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtener suscripción de la organización",
        Description = @"**Propósito:**

Obtiene la suscripción correspondiente a la organización autenticada.
Encabezados Requeridos
- Authorization: Bearer {token_jwt}

---
Respuestas
- 200 OK: Suscripción encontrada.
- 404 Not Found: La organización no posee una suscripción.",
        OperationId = "GetSubscription")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripción encontrada", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Organización no encontrada en el contexto de autenticación")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "La organización no posee una suscripción")]
    
    public async Task<IActionResult> GetSubscription()
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var query = new GetSubscriptionByOrganizationQuery(organizationId);

        var subscription = await subscriptionQueryService.Handle(query);

        if (subscription == null) return NotFound();

        var resource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);

        return Ok(resource);
    }
    
    
    /// <summary>
    /// Obtiene una suscripción por su identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Obtener suscripción por ID",
        Description = @"Propósito:
Obtiene una suscripción específica perteneciente a la organización autenticada.

---
Parámetro de Ruta
- id: GUID de la suscripción.

---
Respuestas
- 200 OK: Suscripción encontrada.
- 404 Not Found: La suscripción no existe o no pertenece a la organización autenticada.",
        OperationId = "GetSubscriptionById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripción encontrada", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Organización no encontrada en el contexto de autenticación")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Suscripción no encontrada")]
    
    public async Task<IActionResult> GetSubscriptionById(Guid id)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var query = new GetSubscriptionByIdQuery(organizationId, id);

        var subscription = await subscriptionQueryService.Handle(query);

        if (subscription == null) return NotFound();

        var resource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);

        return Ok(resource);
    }
    
    
    /// <summary>
    /// Renueva una suscripción existente.
    /// </summary>
    [HttpPut("{id:guid}/renew")]
    [SwaggerOperation(
        Summary = "Renovar suscripción",
        Description = @"Propósito:
Extiende la fecha de finalización de una suscripción perteneciente a la organización autenticada.

---
Encabezados Requeridos
- Authorization: Bearer {token_jwt}
- Content-Type: application/json

---
JSON de Prueba
```json
{
  ""newEndDate"": ""2027-10-03T00:00:00""
}
```

---
Respuesta Exitosa
Retorna la suscripción actualizada con código 200 OK.",
        OperationId = "RenewSubscription")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripción renovada exitosamente", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Suscripción no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La renovación no está permitida para el estado actual de la suscripción")]
    
    public async Task<IActionResult> RenewSubscription(Guid id, [FromBody] RenewSubscriptionResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = RenewSubscriptionCommandFromResourceAssembler
                .ToCommandFromResource(
                    organizationId,
                    id,
                    resource);

            var subscription = await subscriptionCommandService.Handle(command);

            if (subscription == null) return NotFound();

            return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }
    
    
    /// <summary>
    /// Cancela una suscripción.
    /// </summary>
    [HttpPut("{id:guid}/cancel")]
    [SwaggerOperation(
        Summary = "Cancelar suscripción",
        Description = @"**Propósito:**

Cancela una suscripción perteneciente a la organización autenticada.
Este endpoint no requiere request body.

---
Parámetro de Ruta
- id: GUID de la suscripción.

---
Respuesta Exitosa
Retorna la suscripción cancelada con código 200 OK.",
        OperationId = "CancelSubscription")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripción cancelada exitosamente", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Suscripción no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La cancelación no está permitida para el estado actual de la suscripción")]
    
    public async Task<IActionResult> CancelSubscription(Guid id)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = new CancelSubscriptionCommand(organizationId, id);

            var subscription = await subscriptionCommandService.Handle(command);

            if (subscription == null) return NotFound();

            return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }
    
    
    /// <summary>
    /// Marca una suscripción como expirada.
    /// </summary>
    [HttpPut("{id:guid}/expire")]
    [SwaggerOperation(
        Summary = "Expirar suscripción",
        Description = @"**Propósito:**

Marca una suscripción como expirada.
Este endpoint está pensado para la gestión del ciclo de vida de la suscripción
y no requiere request body.

---
Parámetro de Ruta
- id: GUID de la suscripción.

---
Respuesta Exitosa
Retorna la suscripción expirada con código 200 OK.",
        OperationId = "ExpireSubscription")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripción expirada exitosamente", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Suscripción no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La expiración no está permitida para el estado actual de la suscripción")]
    
    public async Task<IActionResult> ExpireSubscription(Guid id)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = new ExpireSubscriptionCommand(organizationId, id);

            var subscription = await subscriptionCommandService.Handle(command);

            if (subscription == null) return NotFound();

            return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }

    private bool TryGetOrganizationId(out Guid organizationId)
    {
        organizationId = Guid.Empty;

        if (HttpContext.Items["OrganizationId"] is Guid id && id != Guid.Empty)
        {
            organizationId = id;
            return true;
        }

        if (HttpContext.Items["OrganizationId"] is string value &&
            Guid.TryParse(value, out id) &&
            id != Guid.Empty)
        {
            organizationId = id;
            return true;
        }

        return false;
    }
}