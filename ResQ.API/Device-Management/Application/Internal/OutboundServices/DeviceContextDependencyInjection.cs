using ResQ.API.Device_Management.Application.Internal.CommandServices;
using ResQ.API.Device_Management.Application.Internal.QueryServices;
using ResQ.API.Device_Management.Domain.Repositories;
using ResQ.API.Device_Management.Domain.Services;
using ResQ.API.Device_Management.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Device_Management.Interfaces.ACL;

namespace ResQ.API.Device_Management.Application.Internal.OutboundServices;

public static class DeviceContextDependencyInjection
{
    public static IServiceCollection AddDeviceManagementContextServices(
        this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IDeviceRepository, DeviceRepository>();

        // Command Services
        services.AddScoped<IDeviceCommandService, DeviceCommandService>();

        // Query Services
        services.AddScoped<IDeviceQueryService, DeviceQueryService>();
        
        // ACL
        services.AddScoped<IDevicesContextFacade, DevicesContextFacade>();

        return services;
    }
}