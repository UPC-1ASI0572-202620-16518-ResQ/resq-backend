using Microsoft.EntityFrameworkCore;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.ValueObjects;
using ResQ.API.Subscriptions.Domain.Repositories;

namespace ResQ.API.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class SubscriptionRepository(AppDbContext context) : BaseRepository<Subscription>(context), ISubscriptionRepository
{
    public async Task<Subscription?> FindByIdAsync(Guid subscriptionId, Guid organizationId)
    {
        var subscription = await context.Set<Subscription>().FindAsync(new SubscriptionId(subscriptionId));

        if (subscription == null) return null;

        return subscription.OrganizationId == organizationId ? subscription : null;
    }

    public async Task<Subscription?> FindByOrganizationIdAsync(Guid organizationId)
    {
        // The active subscription wins; otherwise the most recent one (cancelled or expired)
        return await context.Set<Subscription>()
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.Status == ESubscriptionStatus.Active ? 0 : 1)
            .ThenByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByOrganizationIdAsync(Guid organizationId)
    {
        return await context.Set<Subscription>()
            .AnyAsync(s =>
                s.OrganizationId == organizationId &&
                s.Status == ESubscriptionStatus.Active);
    }
}