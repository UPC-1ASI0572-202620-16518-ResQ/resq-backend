using ResQ.API.Monitoring.Application.Internal.CommandServices;
using ResQ.API.Monitoring.Application.Internal.QueryServices;
using ResQ.API.Monitoring.Domain.Repositories;
using ResQ.API.Monitoring.Domain.Services;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;
using ResQ.API.Monitoring.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Monitoring.Interfaces.ACL;
using ResQ.API.Monitoring.Infrastructure.ThingSpeak;

namespace ResQ.API.Monitoring.Application.Internal.OutboundServices;

public static class MonitoringContextDependencyInjection
{
    public static IServiceCollection AddMonitoringContextServices(this IServiceCollection services)
    {
        // Settings (appsettings section "MonitoringSettings")
        services.AddOptions<MonitoringSettings>().BindConfiguration(MonitoringSettings.SectionName);

        // Read-only ThingSpeak access. No polling or ingestion starts here.
        services.AddOptions<ThingSpeakOptions>().BindConfiguration(ThingSpeakOptions.SectionName);
        services.AddHttpClient<ThingSpeakFeedClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ThingSpeakOptions>>().Value;
            client.BaseAddress = new Uri("https://api.thingspeak.com");
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(settings.RequestTimeoutSeconds, 1, 60));
        });

        // Repositories
        services.AddScoped<IMeasurementRepository, MeasurementRepository>();
        services.AddScoped<IDeviceMonitoringStateRepository, DeviceMonitoringStateRepository>();
        services.AddScoped<IZoneMonitoringStateRepository, ZoneMonitoringStateRepository>();

        // Domain services
        services.AddScoped<IMonitoringStateService, MonitoringStateService>();

        // Command and Query Services
        services.AddScoped<IMonitoringCommandService, MonitoringCommandService>();
        services.AddScoped<IMonitoringQueryService, MonitoringQueryService>();

        // ACL Facade
        services.AddScoped<IMonitoringContextFacade, MonitoringContextFacade>();

        return services;
    }
}