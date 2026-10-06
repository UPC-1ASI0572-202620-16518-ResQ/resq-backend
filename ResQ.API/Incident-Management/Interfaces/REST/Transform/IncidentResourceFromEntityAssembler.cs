using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;

namespace ResQ.API.Incident_Management.Interfaces.REST.Transform;

public static class IncidentResourceFromEntityAssembler
{
    public static IncidentResource ToResourceFromEntity(Incident entity)
    {
        return new IncidentResource(
            entity.Id.Value,
            entity.ZoneId.Value,
            entity.Type.ToString(),
            entity.Level.ToString(),
            entity.Status.ToString(),
            entity.AssignedTo?.Value,
            entity.CreatedAt,
            entity.ResolvedAt,
            entity.ResolutionNotes);
    }
}