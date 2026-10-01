using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class RegisterDeviceCommandFromResourceAssembler
{
    public static RegisterDeviceCommand ToCommandFromResource(Guid organizationId, RegisterDeviceResource resource)
    {
        var specifications = new DeviceSpecifications(
            resource.Specifications.Manufacturer,
            resource.Specifications.Model,
            resource.Specifications.SerialNumber);

        var assignment = new DeviceAssignment(
            resource.Assignment.BuildingId,
            resource.Assignment.ZoneId);

        ExternalDeviceReference? externalReference = null;

        if (resource.ExternalReference != null)
        {
            externalReference = new ExternalDeviceReference(
                resource.ExternalReference.SourceSystem,
                resource.ExternalReference.ExternalDeviceId);
        }

        var capabilities = resource.Capabilities
            .Select(capability =>
                new CapabilityDefinition(
                    capability.Code,
                    capability.Kind,
                    capability.Unit))
            .ToList();

        return new RegisterDeviceCommand(
            organizationId,
            resource.DeviceCode,
            resource.Name,
            resource.Description,
            specifications,
            assignment,
            externalReference,
            capabilities);
    }
}