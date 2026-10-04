using ResQ.API.Alert_Management.Domain.Model.Entities;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that represents an early warning generated from a risk detection.
/// An alert warns before an emergency happens; an ongoing emergency is handled as an Incident
/// in the Incident Management context.
/// </summary>
public class Alert
{
    private readonly List<NotificationDelivery> _deliveries = new();

    /// <summary>
    /// Unique identifier of the alert.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Organization that owns the alert.
    /// </summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Snapshot of the risk detection that originated the alert.
    /// </summary>
    public AlertContext Context { get; private set; } = null!;

    /// <summary>
    /// Date and time when the alert was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; private set; }

    /// <summary>
    /// Notifications sent for this alert.
    /// </summary>
    public IReadOnlyCollection<NotificationDelivery> Deliveries => _deliveries.AsReadOnly();

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected Alert()
    {
    }

    /// <summary>
    /// Generates a new alert and requests one notification delivery per recipient.
    /// </summary>
    public static Alert Generate(Guid organizationId, AlertContext context, IEnumerable<NotificationRecipient> recipients)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(recipients);

        var recipientList = recipients.ToList();

        var duplicated = recipientList
            .GroupBy(recipient => (
                User: recipient.RecipientUserId?.Trim(),
                Channel: recipient.Channel?.Trim().ToUpperInvariant(),
                Destination: recipient.Destination?.Trim()))
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicated is not null)
            throw new ArgumentException($"Recipient '{duplicated.Key.User}' is duplicated for channel '{duplicated.Key.Channel}'.");

        var now = DateTimeOffset.UtcNow;

        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Context = context,
            GeneratedAt = now
        };

        foreach (var recipient in recipientList)
        {
            alert._deliveries.Add(new NotificationDelivery(recipient, now));
        }

        return alert;
    }

    /// <summary>
    /// Records the outcome of one of the notification deliveries.
    /// </summary>
    public NotificationDelivery RecordDeliveryOutcome(Guid deliveryId, ENotificationDeliveryStatus status, string? failureReason)
    {
        var delivery = _deliveries.FirstOrDefault(item => item.Id == deliveryId)
                       ?? throw new KeyNotFoundException("Notification delivery not found.");

        delivery.RecordOutcome(status, failureReason);

        return delivery;
    }
}
