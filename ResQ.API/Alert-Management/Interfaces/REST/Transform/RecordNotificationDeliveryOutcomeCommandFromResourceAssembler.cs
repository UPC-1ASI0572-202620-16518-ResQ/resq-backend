using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class RecordNotificationDeliveryOutcomeCommandFromResourceAssembler
{
    public static RecordNotificationDeliveryOutcomeCommand ToCommandFromResource(Guid organizationId, Guid alertId, Guid deliveryId,
        RecordNotificationDeliveryOutcomeResource resource)
    {
        var status = EnumCode.Parse<ENotificationDeliveryStatus>(resource.Status, "status");

        return new RecordNotificationDeliveryOutcomeCommand(organizationId, alertId, deliveryId, status, resource.FailureReason);
    }
}
