using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class AssignDeviceToLocationCommandFromResourceAssembler
{
    public static AssignDeviceToLocationCommand ToCommandFromResource(Guid organizationId, Guid deviceId,
        AssignDeviceToLocationResource resource)
    {
        return new AssignDeviceToLocationCommand(
            organizationId,
            deviceId,
            resource.BuildingId,
            resource.ZoneId);
    }
}