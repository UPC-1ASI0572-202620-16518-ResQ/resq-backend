using ResQ.API.Subscriptions.Application.Internal.CommandServices;
using ResQ.API.Subscriptions.Application.Internal.QueryServices;
using ResQ.API.Subscriptions.Domain.Repositories;
using ResQ.API.Subscriptions.Domain.Services;
using ResQ.API.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Subscriptions.Application.Internal.OutboundServices;

public static class SubscriptionContextDependencyInjection
{
    public static IServiceCollection AddSubscriptionContextServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // Command Services
        services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();

        // Query Services
        services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();

        return services;
    }
}