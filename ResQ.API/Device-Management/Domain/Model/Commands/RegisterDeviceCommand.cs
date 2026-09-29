using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Commands;

public record RegisterDeviceCommand(Guid OrganizationId, string DeviceCode, string Name, string? Description, DeviceSpecifications Specifications,
    DeviceAssignment Assignment, ExternalDeviceReference? ExternalReference, IReadOnlyCollection<CapabilityDefinition> Capabilities);