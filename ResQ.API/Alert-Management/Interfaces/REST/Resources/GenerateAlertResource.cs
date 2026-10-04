namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Payload to generate an alert from a risk detection.
/// </summary>
public record GenerateAlertResource(
    string RiskDetectionId,
    string RiskTypeCode,
    string SeverityCode,
    Guid? BuildingId,
    Guid? ZoneId,
    DateTimeOffset DetectedAt,
    IEnumerable<NotificationRecipientResource>? Recipients);

/// <summary>
/// Recipient of the alert notification.
/// </summary>
public record NotificationRecipientResource(string RecipientUserId, string Channel, string Destination);
