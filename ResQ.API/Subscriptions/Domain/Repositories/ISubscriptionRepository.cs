using ResQ.API.Shared.Domain.Repositories;
using ResQ.API.Subscriptions.Domain.Model.Aggregates;

namespace ResQ.API.Subscriptions.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<Subscription>
{
    Task<Subscription?> FindByIdAsync(Guid subscriptionId, Guid organizationId);

    Task<Subscription?> FindByOrganizationIdAsync(Guid organizationId);

    Task<bool> ExistsByOrganizationIdAsync(Guid organizationId);
}