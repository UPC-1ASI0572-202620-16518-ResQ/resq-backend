using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Interfaces.REST.Resources;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class ReplaceDeviceCapabilitiesCommandFromResourceAssembler
{
    public static ReplaceDeviceCapabilitiesCommand ToCommandFromResource(Guid organizationId, Guid deviceId,
        ReplaceDeviceCapabilitiesResource resource)
    {
        var capabilities = resource.Capabilities == null
            ? new List<CapabilityDefinition>()
            : resource.Capabilities
                .Select(capability =>
                    new CapabilityDefinition(
                        capability.Code,
                        capability.Kind,
                        capability.Unit))
                .ToList();

        return new ReplaceDeviceCapabilitiesCommand(
            organizationId,
            deviceId,
            capabilities);
    }
}