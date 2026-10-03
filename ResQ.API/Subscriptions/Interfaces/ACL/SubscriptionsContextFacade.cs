using ResQ.API.Subscriptions.Domain.Model.Queries;
using ResQ.API.Subscriptions.Domain.Model.ValueObjects;
using ResQ.API.Subscriptions.Domain.Services;

namespace ResQ.API.Subscriptions.Interfaces.ACL;

public class SubscriptionsContextFacade(ISubscriptionQueryService subscriptionQueryService) : ISubscriptionsContextFacade
{
    public async Task<bool> HasActiveSubscription(Guid organizationId)
    {
        var query = new GetSubscriptionByOrganizationQuery(organizationId);

        var subscription = await subscriptionQueryService.Handle(query);

        return subscription != null && subscription.Status == ESubscriptionStatus.Active;
    }
}