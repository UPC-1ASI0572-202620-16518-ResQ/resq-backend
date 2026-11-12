using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record AssignIncidentToCurrentUserCommand(Guid OrganizationId, Guid IncidentId, int UserId);

public record RecordIncidentHumanActionCommand(
    Guid OrganizationId, Guid IncidentId, int UserId, string ActionCode, string? Notes);

public record VerifyIncidentSafetyCommand(Guid OrganizationId, Guid IncidentId, int UserId);

public record ResolveIncidentAsCurrentUserCommand(
    Guid OrganizationId, Guid IncidentId, int UserId, string ResolutionNotes);

public record ChangeIncidentStatusAsCurrentUserCommand(
    Guid OrganizationId, Guid IncidentId, int UserId, EIncidentStatus Status);
