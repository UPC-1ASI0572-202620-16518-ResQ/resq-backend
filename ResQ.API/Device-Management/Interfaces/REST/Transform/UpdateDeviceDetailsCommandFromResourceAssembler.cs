using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class UpdateDeviceDetailsCommandFromResourceAssembler
{
    public static UpdateDeviceDetailsCommand ToCommandFromResource(Guid organizationId, Guid deviceId,
        UpdateDeviceDetailsResource resource)
    {
        return new UpdateDeviceDetailsCommand(
            organizationId,
            deviceId,
            resource.Name,
            resource.Description,
            resource.Specifications?.Manufacturer,
            resource.Specifications?.Model,
            resource.Specifications?.SerialNumber);
    }
}