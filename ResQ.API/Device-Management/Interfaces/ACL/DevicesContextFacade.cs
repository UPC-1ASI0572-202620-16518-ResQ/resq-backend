using ResQ.API.Device_Management.Domain.Model.Queries;
using ResQ.API.Device_Management.Domain.Services;

namespace ResQ.API.Device_Management.Interfaces.ACL;

public class DevicesContextFacade(IDeviceQueryService deviceQueryService) : IDevicesContextFacade
{
    public async Task<DeviceCatalogEntry?> GetDeviceCatalogEntry(Guid organizationId, Guid deviceId)
    {
        var query = new GetDeviceByIdQuery(organizationId, deviceId);
        var device = await deviceQueryService.Handle(query);

        if (device == null) return null;

        var capabilities = device.Capabilities
            .Select(capability =>
                new DeviceCatalogCapability(
                    capability.Code,
                    capability.Kind,
                    capability.Unit))
            .ToList();

        return new DeviceCatalogEntry(
            device.Id,
            device.OrganizationId,
            device.DeviceCode.Value,
            device.Assignment.BuildingId,
            device.Assignment.ZoneId,
            device.AdministrativeStatus,
            capabilities,
            device.Version);
    }
}