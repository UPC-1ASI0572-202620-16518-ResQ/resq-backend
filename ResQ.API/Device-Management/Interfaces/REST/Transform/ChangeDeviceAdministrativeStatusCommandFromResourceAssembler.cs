using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class ChangeDeviceAdministrativeStatusCommandFromResourceAssembler
{
    public static ChangeDeviceAdministrativeStatusCommand ToCommandFromResource(Guid organizationId, Guid deviceId,
        long expectedVersion, ChangeDeviceAdministrativeStatusResource resource)
    {
        return new ChangeDeviceAdministrativeStatusCommand(
            organizationId,
            deviceId,
            resource.AdministrativeStatus,
            expectedVersion);
    }
}