using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Commands;

public record UpdateDeviceDetailsCommand(Guid OrganizationId, Guid DeviceId, string Name, string? Description, DeviceSpecifications Specifications, long ExpectedVersion);