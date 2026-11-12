namespace ResQ.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
/// Availability reported by Monitoring from the connectivity state of a device.
/// </summary>
public enum EDeviceAvailability
{
    Available,
    Unavailable,
    Unknown
}