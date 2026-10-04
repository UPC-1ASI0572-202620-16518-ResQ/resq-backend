using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record GenerateAlertCommand(
    Guid OrganizationId,
    string RiskDetectionId,
    string RiskTypeCode,
    string SeverityCode,
    Guid? BuildingId,
    Guid? ZoneId,
    DateTimeOffset DetectedAt,
    IReadOnlyCollection<NotificationRecipient> Recipients);
