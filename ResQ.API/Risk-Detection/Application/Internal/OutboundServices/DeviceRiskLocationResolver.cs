using ResQ.API.Device_Management.Interfaces.ACL;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Application.Internal.OutboundServices;

/// <summary>
/// Resolves the location of a detection through the Device Management ACL,
/// without making Risk Detection the owner of devices, buildings or zones.
/// </summary>
public class DeviceRiskLocationResolver(IDevicesContextFacade devicesContextFacade) : IRiskLocationResolver
{
    public async Task<RiskLocation> ResolveAsync(Guid organizationId, Guid deviceId)
    {
        var device = await devicesContextFacade.GetDeviceCatalogEntry(organizationId, deviceId);

        if (device is null || device.BuildingId == Guid.Empty)
            return RiskLocation.Unresolved();

        return RiskLocation.Resolved(device.BuildingId, device.ZoneId);
    }
}
