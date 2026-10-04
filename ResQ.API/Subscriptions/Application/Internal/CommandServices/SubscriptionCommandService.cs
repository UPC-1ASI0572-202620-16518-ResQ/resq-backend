using ResQ.API.Shared.Domain.Repositories;
using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.Commands;
using ResQ.API.Subscriptions.Domain.Repositories;
using ResQ.API.Subscriptions.Domain.Services;

namespace ResQ.API.Subscriptions.Application.Internal.CommandServices;

public class SubscriptionCommandService(ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork) : ISubscriptionCommandService
{
    public async Task<Subscription?> Handle(CreateSubscriptionCommand command)
    {
        var exists = await subscriptionRepository.ExistsByOrganizationIdAsync(command.OrganizationId);

        if (exists) return null;

        var subscription = Subscription.Create(command.OrganizationId, command.StartDate, command.EndDate);

        await subscriptionRepository.AddAsync(subscription);

        await unitOfWork.CompleteAsync();

        return subscription;
    }

    public async Task<Subscription?> Handle(CancelSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId, command.OrganizationId);

        if (subscription == null) return null;

        subscription.Cancel();

        subscriptionRepository.Update(subscription);

        await unitOfWork.CompleteAsync();

        return subscription;
    }

    public async Task<Subscription?> Handle(RenewSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId, command.OrganizationId);

        if (subscription == null) return null;

        subscription.Renew(command.NewEndDate);

        subscriptionRepository.Update(subscription);

        await unitOfWork.CompleteAsync();

        return subscription;
    }

    public async Task<Subscription?> Handle(ExpireSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId, command.OrganizationId);

        if (subscription == null) return null;

        subscription.Expire();

        subscriptionRepository.Update(subscription);

        await unitOfWork.CompleteAsync();

        return subscription;
    }
}