using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Commands;

public record ReplaceDeviceCapabilitiesCommand(Guid OrganizationId, Guid DeviceId, IReadOnlyCollection<CapabilityDefinition> Capabilities, long ExpectedVersion);