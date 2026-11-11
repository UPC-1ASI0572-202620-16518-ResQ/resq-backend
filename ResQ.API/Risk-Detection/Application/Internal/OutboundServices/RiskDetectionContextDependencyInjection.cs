using ResQ.API.Risk_Detection.Application.Internal.CommandServices;
using ResQ.API.Risk_Detection.Application.Internal.QueryServices;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;
using ResQ.API.Risk_Detection.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Risk_Detection.Interfaces.ACL;

namespace ResQ.API.Risk_Detection.Application.Internal.OutboundServices;

public static class RiskDetectionContextDependencyInjection
{
    public static IServiceCollection AddRiskDetectionContextServices(this IServiceCollection services)
    {
        // Settings (appsettings section "RiskDetectionSettings")
        services.AddOptions<RiskDetectionSettings>().BindConfiguration(RiskDetectionSettings.SectionName);

        // Repositories
        services.AddScoped<IDetectionRuleRepository, DetectionRuleRepository>();
        services.AddScoped<IRiskDetectionRepository, RiskDetectionRepository>();

        // Outbound services (Device Management and Alert Management ACLs)
        services.AddScoped<IRiskLocationResolver, DeviceRiskLocationResolver>();
        services.AddScoped<IRiskDetectionEventPublisher, AlertManagementRiskDetectionEventPublisher>();

        // Command Services
        services.AddScoped<IDetectionRuleCommandService, DetectionRuleCommandService>();
        services.AddScoped<IRiskDetectionCommandService, RiskDetectionCommandService>();

        // Query Services
        services.AddScoped<IDetectionRuleQueryService, DetectionRuleQueryService>();
        services.AddScoped<IRiskDetectionQueryService, RiskDetectionQueryService>();

        // ACL Facade
        services.AddScoped<IRiskDetectionContextFacade, RiskDetectionContextFacade>();

        return services;
    }
}
