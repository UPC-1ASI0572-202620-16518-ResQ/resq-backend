using Microsoft.EntityFrameworkCore;
using ResQ.API.Subscriptions.Domain.Model.Aggregates;

namespace ResQ.API.Subscriptions.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplySubscriptionConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Subscription>().ToTable("Subscriptions");

        builder.Entity<Subscription>().HasKey(s => s.Id);

        // SubscriptionId Value Object
        builder.Entity<Subscription>().Property(s => s.Id).HasConversion(subscriptionId => subscriptionId.Value, value => new Domain.Model.ValueObjects.SubscriptionId(value)).ValueGeneratedNever();

        // External reference to Organization Management
        builder.Entity<Subscription>().Property(s => s.OrganizationId).IsRequired();

        // Subscription lifecycle
        builder.Entity<Subscription>().Property(s => s.Status).HasConversion<string>().IsRequired();

        // Subscription period
        builder.Entity<Subscription>().Property(s => s.StartDate).IsRequired();
        builder.Entity<Subscription>().Property(s => s.EndDate).IsRequired();

        return builder;
    }
}