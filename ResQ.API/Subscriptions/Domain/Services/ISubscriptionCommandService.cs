using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.Commands;

namespace ResQ.API.Subscriptions.Domain.Services;

public interface ISubscriptionCommandService
{
    Task<Subscription?> Handle(CreateSubscriptionCommand command);

    Task<Subscription?> Handle(CancelSubscriptionCommand command);

    Task<Subscription?> Handle(RenewSubscriptionCommand command);

    Task<Subscription?> Handle(ExpireSubscriptionCommand command);
}