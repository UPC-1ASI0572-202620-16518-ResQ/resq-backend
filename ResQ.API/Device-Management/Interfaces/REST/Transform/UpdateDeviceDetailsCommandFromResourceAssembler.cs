using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class UpdateDeviceDetailsCommandFromResourceAssembler
{
    public static UpdateDeviceDetailsCommand ToCommandFromResource(Guid organizationId, Guid deviceId,
        long expectedVersion, UpdateDeviceDetailsResource resource)
    {
        var specifications = new DeviceSpecifications(
            resource.Specifications.Manufacturer,
            resource.Specifications.Model,
            resource.Specifications.SerialNumber);

        return new UpdateDeviceDetailsCommand(
            organizationId,
            deviceId,
            resource.Name,
            resource.Description,
            specifications,
            expectedVersion);
    }
}