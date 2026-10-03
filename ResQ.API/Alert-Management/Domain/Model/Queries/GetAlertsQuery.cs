namespace ResQ.API.Alert_Management.Domain.Model.Queries;

public record GetAlertsQuery(
    Guid OrganizationId,
    Guid? BuildingId,
    Guid? ZoneId,
    string? RiskTypeCode,
    DateTimeOffset? From,
    DateTimeOffset? To);
