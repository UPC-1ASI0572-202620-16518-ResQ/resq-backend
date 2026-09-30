using Microsoft.AspNetCore.Mvc;
using ResQ.API.Incident_Management.Domain.Model.Queries;
using ResQ.API.Incident_Management.Domain.Services;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;
using ResQ.API.Incident_Management.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Incident_Management.Interfaces.REST;

[ApiController]
[Route("api/v1/incidents")]
[Produces("application/json")]
public class IncidentsController(IIncidentCommandService incidentCommandService, IIncidentQueryService incidentQueryService) : ControllerBase
{


    [HttpPost]
    [SwaggerOperation(Summary = "Create an incident", Description = "Registers a new incident in the system.", OperationId = "CreateIncident")]
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



    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get incident by id", Description = "Returns an incident by its identifier.", OperationId = "GetIncidentById")]
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
    
    [HttpPut("{id:guid}/status")]
    [SwaggerOperation(Summary = "Change incident status", Description = "Updates the current status of an incident.", OperationId = "ChangeIncidentStatus")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] UpdateIncidentStatusResource resource)
    {

        var command = UpdateIncidentStatusCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
    
    [HttpPut("{id:guid}/assign")]
    [SwaggerOperation(Summary = "Assign attendant", Description = "Assigns an attendant to an active incident.", OperationId = "AssignIncident")]
    public async Task<IActionResult> AssignIncident(Guid id, [FromBody] AssignIncidentResource resource)
    {
        var command = AssignIncidentCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
    
    [HttpPut("{id:guid}/resolve")]
    [SwaggerOperation(Summary = "Resolve incident", Description = "Marks an incident as resolved.", OperationId = "ResolveIncident")]
    public async Task<IActionResult> ResolveIncident(Guid id, [FromBody] ResolveIncidentResource resource)
    {
        var command = ResolveIncidentCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var incident = await incidentCommandService.Handle(command);
        
        if (incident == null)
            return NotFound();
        
        return Ok(IncidentResourceFromEntityAssembler.ToResourceFromEntity(incident));
    }
}