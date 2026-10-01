namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record RegisterDeviceResource(string DeviceCode, string Name, string? Description,
    DeviceSpecificationsResource Specifications, DeviceAssignmentResource Assignment,
    ExternalDeviceReferenceResource? ExternalReference, List<CapabilityDefinitionResource> Capabilities);