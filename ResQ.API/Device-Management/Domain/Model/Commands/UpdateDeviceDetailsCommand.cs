namespace ResQ.API.Device_Management.Domain.Model.Commands;

public record UpdateDeviceDetailsCommand(
    Guid OrganizationId,
    Guid DeviceId,
    string? Name,
    string? Description,
    string? Manufacturer,
    string? Model,
    string? SerialNumber);