namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Alert returned by the API. Matches AlertResourceDto in the frontend.
/// </summary>
public record AlertResource(
    Guid AlertId,
    Guid OrganizationId,
    AlertContextResource Context,
    DateTimeOffset GeneratedAt,
    IEnumerable<NotificationDeliveryResource> Deliveries);

/// <summary>
/// Risk detection snapshot of an alert. Matches AlertContextResourceDto in the frontend.
/// </summary>
public record AlertContextResource(
    string RiskDetectionId,
    string RiskTypeCode,
    string SeverityCode,
    Guid? BuildingId,
    Guid? ZoneId,
    DateTimeOffset DetectedAt);

/// <summary>
/// Notification delivery of an alert. Matches NotificationDeliveryResourceDto in the frontend.
/// Status is PENDING, DELIVERED or FAILED.
/// </summary>
public record NotificationDeliveryResource(
    Guid DeliveryId,
    string RecipientUserId,
    string Channel,
    string Destination,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string? FailureReason);
