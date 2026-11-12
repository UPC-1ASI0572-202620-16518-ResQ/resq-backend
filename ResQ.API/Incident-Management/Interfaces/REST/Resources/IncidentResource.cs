namespace ResQ.API.Incident_Management.Interfaces.REST.Resources;

public record IncidentResource(
    Guid Id,
    Guid OrganizationId,
    Guid ZoneId,
    Guid? RiskDetectionId,
    Guid? AlertId,
    string Type,
    string Level,
    string Status,
    Guid? AssignedTo,
    int? AssignedUserId,
    DateTimeOffset? AssignedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    int? ResolvedByUserId,
    DateTimeOffset? ClosedAt,
    int? ClosedByUserId,
    string? ResolutionNotes,
    bool SafetyVerified,
    IEnumerable<IncidentHumanActionResource> HumanActions,
    IEnumerable<IncidentSafetyVerificationResource> SafetyVerifications);

public record IncidentHumanActionResource(Guid Id, int UserId, string ActionCode, string? Notes, DateTimeOffset PerformedAt);

public record IncidentSafetyVerificationResource(
    Guid Id, int VerifiedByUserId, bool Safe, string Reason, Guid? DeviceId, Guid? MeasurementId,
    decimal? MeasuredValue, DateTimeOffset? MeasuredAt, string? RuleSnapshot, DateTimeOffset VerifiedAt);
