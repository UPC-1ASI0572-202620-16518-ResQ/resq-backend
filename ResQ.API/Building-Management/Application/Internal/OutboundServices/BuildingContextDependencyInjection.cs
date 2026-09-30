using ResQ.API.Building_Management.Application.Internal.CommandServices;
using ResQ.API.Building_Management.Application.Internal.QueryServices;
using ResQ.API.Building_Management.Domain.Repositories;
using ResQ.API.Building_Management.Domain.Services;
using ResQ.API.Building_Management.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Building_Management.Application.Internal.OutboundServices;

public static class BuildingContextDependencyInjection
{
    public static IServiceCollection AddBuildingManagementContextServices(
        this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IBuildingRepository, BuildingRepository>();

        // Command Services
        services.AddScoped<IBuildingCommandService, BuildingCommandService>();

        // Query Services
        services.AddScoped<IBuildingQueryService, BuildingQueryService>();

        // ACL Facade
        services.AddScoped<ResQ.API.Building_Management.Interfaces.ACL.IBuildingsContextFacade,
            ResQ.API.Building_Management.Interfaces.ACL.BuildingsContextFacade>();

        return services;
    }
}
