using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.Queries;
using ResQ.API.Subscriptions.Domain.Repositories;
using ResQ.API.Subscriptions.Domain.Services;

namespace ResQ.API.Subscriptions.Application.Internal.QueryServices;

public class SubscriptionQueryService(ISubscriptionRepository subscriptionRepository) : ISubscriptionQueryService
{
    public async Task<Subscription?> Handle(GetSubscriptionByIdQuery query)
    {
        return await subscriptionRepository.FindByIdAsync(query.SubscriptionId, query.OrganizationId);
    }

    public async Task<Subscription?> Handle(GetSubscriptionByOrganizationQuery query)
    {
        return await subscriptionRepository.FindByOrganizationIdAsync(query.OrganizationId);
    }
}