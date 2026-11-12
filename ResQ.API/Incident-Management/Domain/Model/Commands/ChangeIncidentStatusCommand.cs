using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record ChangeIncidentStatusCommand(Guid OrganizationId, IncidentId IncidentId, EIncidentStatus Status);
