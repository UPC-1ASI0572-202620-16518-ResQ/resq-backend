using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Domain.Model.ValueObjects;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;

namespace ResQ.API.Incident_Management.Interfaces.REST.Transform;

public static class UpdateIncidentStatusCommandFromResourceAssembler
{
    public static ChangeIncidentStatusCommand ToCommandFromResource(Guid incidentId, UpdateIncidentStatusResource resource)
    {
        if (!Enum.TryParse<EIncidentStatus>(resource.Status, true, out var status))
        {
            throw new ArgumentException("Invalid incident status.");
        }

        return new ChangeIncidentStatusCommand(new IncidentId(incidentId), status);
    }
}