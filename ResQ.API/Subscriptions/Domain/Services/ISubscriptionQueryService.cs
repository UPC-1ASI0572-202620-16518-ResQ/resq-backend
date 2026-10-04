using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.Queries;

namespace ResQ.API.Subscriptions.Domain.Services;

public interface ISubscriptionQueryService
{
    Task<Subscription?> Handle(GetSubscriptionByIdQuery query);

    Task<Subscription?> Handle(GetSubscriptionByOrganizationQuery query);
}