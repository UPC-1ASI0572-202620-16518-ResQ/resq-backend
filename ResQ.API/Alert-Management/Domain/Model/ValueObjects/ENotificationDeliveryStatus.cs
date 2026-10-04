namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Delivery state of a notification sent for an alert.
/// </summary>
public enum ENotificationDeliveryStatus
{
    Pending,
    Delivered,
    Failed
}
