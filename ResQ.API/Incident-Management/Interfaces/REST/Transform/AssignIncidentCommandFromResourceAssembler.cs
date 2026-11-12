using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;

namespace ResQ.API.Incident_Management.Interfaces.REST.Transform;

public static class AssignIncidentCommandFromResourceAssembler
{
    public static AssignIncidentCommand ToCommandFromResource(Guid organizationId, Guid incidentId, AssignIncidentResource resource)
    {
        return new AssignIncidentCommand(organizationId, incidentId, resource.AttendantId);
    }
}