using ResQ.API.Incident_Management.Application.Internal.CommandServices;
using ResQ.API.Incident_Management.Application.Internal.QueryServices;
using ResQ.API.Incident_Management.Domain.Repositories;
using ResQ.API.Incident_Management.Domain.Services;
using ResQ.API.Incident_Management.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Incident_Management.Application.Internal.OutboundServices;

public static class IncidentContextDependencyInjection
{
    public static IServiceCollection AddIncidentManagementContextServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IIncidentRepository, IncidentRepository>();

        // Command Services
        services.AddScoped<IIncidentCommandService, IncidentCommandService>();

        // Query Services
        services.AddScoped<IIncidentQueryService, IncidentQueryService>();

        // ACL
        services.AddScoped<ResQ.API.Incident_Management.Interfaces.ACL.IIncidentContextFacade,
            ResQ.API.Incident_Management.Interfaces.ACL.IncidentContextFacade>();

        return services;
    }
}