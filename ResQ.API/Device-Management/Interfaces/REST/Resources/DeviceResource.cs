using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record DeviceResource(
    Guid Id,
    Guid OrganizationId,
    string DeviceCode,
    string Name,
    string? Description,
    DeviceSpecificationsResource Specifications,
    DeviceAssignmentResource Assignment,
    ExternalDeviceReferenceResource? ExternalReference,
    EDeviceAdministrativeStatus AdministrativeStatus,
    List<DeviceCapabilityResource> Capabilities,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);