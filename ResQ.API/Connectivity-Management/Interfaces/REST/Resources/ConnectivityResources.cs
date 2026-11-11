namespace ResQ.API.Connectivity_Management.Interfaces.REST.Resources;

/// <summary>
/// Connection status of a sensor. Matches SensorConnectionResourceDto in the frontend.
/// Status is ONLINE, OFFLINE or TIMEOUT; signalStrength is in dBm.
/// </summary>
public record SensorConnectionResource(
    Guid SensorId,
    string Status,
    DateTimeOffset LastHeartbeatTime,
    int? SignalStrength);

/// <summary>
/// Heartbeat sent by an IoT device.
/// </summary>
public record RegisterHeartbeatResource(
    Guid SensorId,
    int? SignalStrength);
