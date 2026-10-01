using Microsoft.AspNetCore.Mvc;
using ResQ.API.Incident_Management.Domain.Model.Queries;
using ResQ.API.Incident_Management.Domain.Services;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;
using ResQ.API.Incident_Management.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Incident_Management.Interfaces.REST;

/// <summary>
/// Controlador REST para la gestión del ciclo de vida de incidentes (Incident Management Bounded Context).
/// </summary>
[ApiController]
[Route("api/v1/incidents")]
[Produces("application/json")]
public class IncidentsController(IIncidentCommandService incidentCommandService, IIncidentQueryService incidentQueryService) : ControllerBase
{

    /// <summary>
    /// Registra un nuevo incidente en una zona determinada.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create an incident", 
        Description = @"**Propósito:**
Registra un nuevo incidente asociado a una zona. El incidente se crea automáticamente en estado **Active**.

---
### Encabezados Requeridos (Headers)

* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba

```json
{
  ""zoneId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
  ""type"": ""Fire"",
  ""level"": ""High""
}
```

---
Tipos de Riesgo Soportados
- Earthquake
- Fire
- GasLeak
- Unknown

---
Niveles de Riesgo Soportados
- Low
- Medium
- High
- Critical

---
Respuesta Exitosa (201 Created)
Retorna el incidente creado con su identificador, zona, tipo, nivel y estado actual.", 
        OperationId = "CreateIncident")]
    [SwaggerResponse(StatusCodes.Status201Created, "Incident created")]
    public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentResource resource)
    {
        var command = CreateIncidentCommandFromResourceAssembler.ToCommandFromResource(resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return BadRequest();
        
        var response = IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident);
        
        return CreatedAtAction(
            nameof(GetIncidentById),
            new
            {
                id = incident.Id.Value
            },
            response);
    }


    /// <summary>
    /// Obtiene un incidente específico mediante su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get incident by id", 
        Description = @"**Propósito:**
Obtiene la información de un incidente registrado mediante su identificador único.

---

Encabezados Requeridos (Headers)
- Authorization: Bearer {token_jwt}

---

Parámetro de Ruta (Path Param)
- id: GUID correspondiente al identificador del incidente.

```
Ejemplo:
5bb184bd-e1bb-4248-a3e8-07fde5cdf854
```

---
Respuesta Exitosa (200 OK)

Retorna la información actual del incidente, incluyendo:
- Identificador del incidente.
- Identificador de la zona.
- Tipo de riesgo.
- Nivel de riesgo.
- Estado actual.",
        OperationId = "GetIncidentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Incident found")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
    public async Task<IActionResult> GetIncidentById(Guid id)
    {
        var query = new GetIncidentByIdQuery(id);
        var incident = await incidentQueryService.Handle(query);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
    
    /// <summary>
    /// Cambia el estado actual de un incidente.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [SwaggerOperation(Summary = "Change incident status", 
        Description = @"Propósito:
Actualiza el estado actual de un incidente de acuerdo con las reglas de transición definidas por el dominio.

---
Encabezados Requeridos (Headers)
- Authorization: Bearer {token_jwt}
- Content-Type: application/json

---
Parámetro de Ruta
- id: GUID correspondiente al incidente.

---
```JSON de Prueba
{
  ""status"": ""InProgress""
}
```

---
Estados Soportados
- Active
- InProgress
- Resolved
- Closed

---
El agregado Incident controla las transiciones permitidas entre estos estados. Por lo tanto, una solicitud no puede realizar arbitrariamente cualquier cambio de estado.

Respuesta Exitosa (200 OK)

Retorna el incidente con su nuevo estado.", 
        OperationId = "ChangeIncidentStatus")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] UpdateIncidentStatusResource resource)
    {

        var command = UpdateIncidentStatusCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
    
    /// <summary>
    /// Asigna un responsable a un incidente activo.
    /// </summary>
    [HttpPut("{id:guid}/assign")]
    [SwaggerOperation(Summary = "Assign attendant", 
        Description = @"**Propósito:**
Asigna un responsable a un incidente que se encuentra activo.

Al realizar la asignación, el incidente pasa a estado InProgress.

---
Encabezados Requeridos (Headers)
- Authorization: Bearer {token_jwt}
- Content-Type: application/json

---
Parámetro de Ruta
- id: GUID correspondiente al incidente.

---
```JSON de Prueba
{
  ""attendantId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
}
```

---
Regla de Negocio
Solo un incidente en estado Active puede recibir una asignación.

Al asignar correctamente un responsable:
Active → InProgress

---
Respuesta Exitosa (200 OK)

Retorna el incidente actualizado con su nuevo estado y responsable asignado.", 
        OperationId = "AssignIncident")]
    public async Task<IActionResult> AssignIncident(Guid id, [FromBody] AssignIncidentResource resource)
    {
        var command = AssignIncidentCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
    
    /// <summary>
    /// Resuelve un incidente registrando las notas de resolución.
    /// </summary>
    [HttpPut("{id:guid}/resolve")]
    [SwaggerOperation(Summary = "Resolve incident", 
        Description = @"**Propósito:**
Registra la resolución de un incidente y almacena las notas correspondientes.
La resolución registra la información necesaria para conservar evidencia del tratamiento realizado.

---
Encabezados Requeridos (Headers)
- Authorization: Bearer {token_jwt}
- Content-Type: application/json

---
Parámetro de Ruta
- id: GUID correspondiente al incidente.

---
```JSON de Prueba
{
  ""resolutionNotes"": ""Se verificó la zona afectada y se controló la situación.""
}
```

---
Regla de Negocio
El incidente debe encontrarse en un estado que permita su resolución.
Las notas de resolución son obligatorias y no pueden estar vacías.
Cuando la resolución se completa, el incidente pasa a estado Resolved y se registra la fecha de resolución.

---
Respuesta Exitosa (200 OK)

Retorna el incidente actualizado con estado Resolved.",
        OperationId = "ResolveIncident")]
    public async Task<IActionResult> ResolveIncident(Guid id, [FromBody] ResolveIncidentResource resource)
    {
        var command = ResolveIncidentCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
}