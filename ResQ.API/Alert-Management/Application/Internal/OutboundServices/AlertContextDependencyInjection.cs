using ResQ.API.Alert_Management.Application.Internal.CommandServices;
using ResQ.API.Alert_Management.Application.Internal.QueryServices;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Alert_Management.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Alert_Management.Interfaces.ACL;

namespace ResQ.API.Alert_Management.Application.Internal.OutboundServices;

public static class AlertContextDependencyInjection
{
    public static IServiceCollection AddAlertManagementContextServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IResponsePolicyRepository, ResponsePolicyRepository>();
        services.AddScoped<IResponseExecutionRepository, ResponseExecutionRepository>();

        // Command Services
        services.AddScoped<IAlertCommandService, AlertCommandService>();
        services.AddScoped<IResponsePolicyCommandService, ResponsePolicyCommandService>();
        services.AddScoped<IResponseExecutionCommandService, ResponseExecutionCommandService>();

        // Query Services
        services.AddScoped<IAlertQueryService, AlertQueryService>();
        services.AddScoped<IResponsePolicyQueryService, ResponsePolicyQueryService>();
        services.AddScoped<IResponseExecutionQueryService, ResponseExecutionQueryService>();

        // ACL Facade
        services.AddScoped<IAlertsContextFacade, AlertsContextFacade>();

        return services;
    }
}
