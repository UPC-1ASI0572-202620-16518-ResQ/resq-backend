namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record DeviceSpecificationsResource(
    string? Manufacturer,
    string? Model,
    string? SerialNumber);