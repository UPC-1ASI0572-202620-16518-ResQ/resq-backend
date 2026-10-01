using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;

namespace ResQ.API.Incident_Management.Interfaces.REST.Transform;

public static class ResolveIncidentCommandFromResourceAssembler
{
    public static ResolveIncidentCommand ToCommandFromResource(Guid incidentId, ResolveIncidentResource resource)
    {
        return new ResolveIncidentCommand(incidentId, resource.ResolutionNotes);
    }
}