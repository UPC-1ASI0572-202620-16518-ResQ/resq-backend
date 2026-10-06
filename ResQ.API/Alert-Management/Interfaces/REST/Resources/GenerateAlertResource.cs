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
    IEnumerable<NotificationRecipientResource>? Recipients,
    IEnumerable<RequestResponseActionResource>? ResponseActions);

/// <summary>
/// Recipient of the alert notification.
/// </summary>
public record NotificationRecipientResource(string RecipientUserId, string Channel, string Destination);

/// <summary>
/// Response action requested with the alert. AuthorizationMode is AUTOMATIC or HUMAN_REQUIRED.
/// </summary>
public record RequestResponseActionResource(
    string ActionCode,
    Guid TargetDeviceId,
    string TargetCapabilityCode,
    string AuthorizationMode,
    bool Critical);
