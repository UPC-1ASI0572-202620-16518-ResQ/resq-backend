namespace ResQ.API.Connectivity_Management.Interfaces.ACL;

/// <summary>
/// Anti-Corruption Layer (ACL) interface exposed to other bounded contexts
/// (e.g. Monitoring can ignore readings of disconnected sensors, Device Management can force a sensor OFFLINE).
/// </summary>
public interface IConnectivityContextFacade
{
    /// <summary>
    /// Returns the connection status code (ONLINE, OFFLINE, TIMEOUT) of a sensor, or null if it never sent a heartbeat.
    /// </summary>
    Task<string?> GetSensorConnectionStatusAsync(Guid organizationId, Guid sensorId);

    /// <summary>
    /// Forces a sensor to OFFLINE. Returns false when the sensor never sent a heartbeat.
    /// </summary>
    Task<bool> MarkSensorOfflineAsync(Guid organizationId, Guid sensorId);
}
