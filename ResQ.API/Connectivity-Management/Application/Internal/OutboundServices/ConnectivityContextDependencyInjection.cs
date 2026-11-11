using ResQ.API.Connectivity_Management.Application.Internal.CommandServices;
using ResQ.API.Connectivity_Management.Application.Internal.EventHandlers;
using ResQ.API.Connectivity_Management.Application.Internal.QueryServices;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Repositories;
using ResQ.API.Connectivity_Management.Domain.Services;
using ResQ.API.Connectivity_Management.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Connectivity_Management.Interfaces.ACL;

namespace ResQ.API.Connectivity_Management.Application.Internal.OutboundServices;

public static class ConnectivityContextDependencyInjection
{
    public static IServiceCollection AddConnectivityManagementContextServices(this IServiceCollection services)
    {
        // Settings (appsettings section "ConnectivitySettings")
        services.AddOptions<ConnectivitySettings>().BindConfiguration(ConnectivitySettings.SectionName);

        // Repositories
        services.AddScoped<ISensorConnectionRepository, SensorConnectionRepository>();

        // Command Services
        services.AddScoped<IConnectivityCommandService, ConnectivityCommandService>();

        // Query Services
        services.AddScoped<IConnectivityQueryService, ConnectivityQueryService>();

        // ACL Facade
        services.AddScoped<IConnectivityContextFacade, ConnectivityContextFacade>();

        // Periodic evaluation of sensor timeouts
        services.AddHostedService<SensorTimeoutBackgroundService>();

        return services;
    }
}
