namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record UpdateDeviceDetailsResource(string Name, string? Description, DeviceSpecificationsResource Specifications);