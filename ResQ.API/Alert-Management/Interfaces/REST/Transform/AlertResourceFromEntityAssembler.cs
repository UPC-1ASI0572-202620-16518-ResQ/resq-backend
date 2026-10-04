using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class AlertResourceFromEntityAssembler
{
    public static AlertResource ToResourceFromEntity(Alert entity)
    {
        var context = new AlertContextResource(
            entity.Context.RiskDetectionId,
            entity.Context.RiskTypeCode,
            entity.Context.SeverityCode,
            entity.Context.BuildingId,
            entity.Context.ZoneId,
            entity.Context.DetectedAt);

        var deliveries = entity.Deliveries
            .OrderBy(delivery => delivery.RequestedAt)
            .Select(delivery => new NotificationDeliveryResource(
                delivery.Id,
                delivery.RecipientUserId,
                delivery.Channel,
                delivery.Destination,
                EnumCode.ToCode(delivery.Status),
                delivery.RequestedAt,
                delivery.CompletedAt,
                delivery.FailureReason))
            .ToList();

        return new AlertResource(entity.Id, entity.OrganizationId, context, entity.GeneratedAt, deliveries);
    }
}
