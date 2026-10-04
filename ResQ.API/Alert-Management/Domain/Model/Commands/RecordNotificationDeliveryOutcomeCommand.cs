using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record RecordNotificationDeliveryOutcomeCommand(
    Guid OrganizationId,
    Guid AlertId,
    Guid DeliveryId,
    ENotificationDeliveryStatus Status,
    string? FailureReason);
