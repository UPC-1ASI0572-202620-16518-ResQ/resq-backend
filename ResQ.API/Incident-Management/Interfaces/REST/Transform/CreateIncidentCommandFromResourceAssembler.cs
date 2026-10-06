using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;

namespace ResQ.API.Incident_Management.Interfaces.REST.Transform;

public static class CreateIncidentCommandFromResourceAssembler
{
    public static CreateIncidentCommand ToCommandFromResource(Guid organizationId, CreateIncidentResource resource)
    {
        return new CreateIncidentCommand(organizationId, resource.ZoneId, resource.Type, resource.Level);
    }
}