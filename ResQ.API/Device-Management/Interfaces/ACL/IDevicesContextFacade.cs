namespace ResQ.API.Device_Management.Interfaces.ACL;

public interface IDevicesContextFacade
{
    Task<DeviceCatalogEntry?> GetDeviceCatalogEntry(Guid organizationId, Guid deviceId);
}