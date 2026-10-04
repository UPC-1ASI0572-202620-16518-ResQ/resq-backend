namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Recipient of an alert notification and the channel used to reach them.
/// </summary>
public record NotificationRecipient(string RecipientUserId, string Channel, string Destination);
