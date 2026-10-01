using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Interfaces.ACL;

public record DeviceCatalogEntry(
    Guid DeviceId,
    Guid OrganizationId,
    string DeviceCode,
    Guid BuildingId,
    Guid? ZoneId,
    EDeviceAdministrativeStatus AdministrativeStatus,
    IReadOnlyCollection<DeviceCatalogCapability> Capabilities,
    long Version);

public record DeviceCatalogCapability(
    string Code,
    ECapabilityKind Kind,
    string? Unit);