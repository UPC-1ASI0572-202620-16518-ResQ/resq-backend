namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Outcome reported by the notification provider. Status is DELIVERED or FAILED.
/// </summary>
public record RecordNotificationDeliveryOutcomeResource(string Status, string? FailureReason);
