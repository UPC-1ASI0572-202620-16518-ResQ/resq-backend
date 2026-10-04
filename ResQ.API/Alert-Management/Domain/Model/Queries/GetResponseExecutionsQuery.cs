using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Queries;

public record GetResponseExecutionsQuery(
    Guid OrganizationId,
    string? RiskDetectionId,
    Guid? AlertId,
    EResponseExecutionStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To);
