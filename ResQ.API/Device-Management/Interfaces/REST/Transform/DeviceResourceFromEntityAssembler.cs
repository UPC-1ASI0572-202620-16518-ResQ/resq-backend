using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class DeviceResourceFromEntityAssembler
{
    public static DeviceResource ToResourceFromEntity(Device entity)
    {
        var specifications = new DeviceSpecificationsResource(
            entity.Specifications.Manufacturer,
            entity.Specifications.Model,
            entity.Specifications.SerialNumber);

        var assignment = new DeviceAssignmentResource(
            entity.Assignment.BuildingId,
            entity.Assignment.ZoneId);

        ExternalDeviceReferenceResource? externalReference = null;

        if (entity.ExternalReference != null)
        {
            externalReference = new ExternalDeviceReferenceResource(
                entity.ExternalReference.SourceSystem,
                entity.ExternalReference.ExternalDeviceId);
        }

        var capabilities = entity.Capabilities
            .Select(capability =>
                new DeviceCapabilityResource(
                    capability.Id,
                    capability.Code,
                    capability.Kind,
                    capability.Unit))
            .ToList();

        return new DeviceResource(
            entity.Id,
            entity.OrganizationId,
            entity.DeviceCode.Value,
            entity.Name,
            entity.Description,
            specifications,
            assignment,
            externalReference,
            entity.AdministrativeStatus,
            capabilities,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Version);
    }
}