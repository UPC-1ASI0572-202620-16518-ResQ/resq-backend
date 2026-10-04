using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Entities;

/// <summary>
/// Notification sent to one recipient for an Alert. Belongs to the Alert aggregate.
/// </summary>
public class NotificationDelivery
{
    public Guid Id { get; private set; }

    public string RecipientUserId { get; private set; } = string.Empty;

    public string Channel { get; private set; } = string.Empty;

    public string Destination { get; private set; } = string.Empty;

    public ENotificationDeliveryStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? FailureReason { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected NotificationDelivery()
    {
    }

    internal NotificationDelivery(NotificationRecipient recipient, DateTimeOffset requestedAt)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (string.IsNullOrWhiteSpace(recipient.RecipientUserId))
            throw new ArgumentException("Recipient user id is required.");

        if (string.IsNullOrWhiteSpace(recipient.Channel))
            throw new ArgumentException("Notification channel is required.");

        if (string.IsNullOrWhiteSpace(recipient.Destination))
            throw new ArgumentException("Notification destination is required.");

        if (recipient.RecipientUserId.Trim().Length > 64)
            throw new ArgumentException("Recipient user id cannot exceed 64 characters.");

        if (recipient.Channel.Trim().Length > 30)
            throw new ArgumentException("Notification channel cannot exceed 30 characters.");

        if (recipient.Destination.Trim().Length > 200)
            throw new ArgumentException("Notification destination cannot exceed 200 characters.");

        Id = Guid.NewGuid();
        RecipientUserId = recipient.RecipientUserId.Trim();
        Channel = recipient.Channel.Trim().ToUpperInvariant();
        Destination = recipient.Destination.Trim();
        Status = ENotificationDeliveryStatus.Pending;
        RequestedAt = requestedAt;
    }

    /// <summary>
    /// Records the outcome reported by the notification provider.
    /// </summary>
    internal void RecordOutcome(ENotificationDeliveryStatus status, string? failureReason)
    {
        if (Status != ENotificationDeliveryStatus.Pending)
            throw new InvalidOperationException("The delivery outcome has already been recorded.");

        switch (status)
        {
            case ENotificationDeliveryStatus.Delivered:
                FailureReason = null;
                break;
            case ENotificationDeliveryStatus.Failed:
                if (string.IsNullOrWhiteSpace(failureReason))
                    throw new ArgumentException("A failure reason is required when the delivery fails.");
                if (failureReason.Trim().Length > 500)
                    throw new ArgumentException("Failure reason cannot exceed 500 characters.");
                FailureReason = failureReason.Trim();
                break;
            default:
                throw new ArgumentException("The delivery outcome must be DELIVERED or FAILED.");
        }

        Status = status;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
