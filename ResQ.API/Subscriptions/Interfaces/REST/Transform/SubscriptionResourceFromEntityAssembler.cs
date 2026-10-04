using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Interfaces.REST.Resources;

namespace ResQ.API.Subscriptions.Interfaces.REST.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription entity)
    {
        return new SubscriptionResource(entity.Id.Value, entity.OrganizationId, entity.Status, entity.StartDate, entity.EndDate);
    }
}