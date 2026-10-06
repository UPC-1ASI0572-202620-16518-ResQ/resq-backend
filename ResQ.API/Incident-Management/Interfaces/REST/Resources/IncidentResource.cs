namespace ResQ.API.Incident_Management.Interfaces.REST.Resources;

public record IncidentResource(
    Guid Id,
    Guid ZoneId,
    string Type,
    string Level,
    string Status,
    Guid? AssignedTo,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNotes);