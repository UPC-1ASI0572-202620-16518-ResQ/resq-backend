namespace ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

/// <summary>
/// Connectivity configuration, bound from the "ConnectivitySettings" section of appsettings.
/// </summary>
public class ConnectivitySettings
{
    public const string SectionName = "ConnectivitySettings";

    /// <summary>
    /// Seconds without heartbeats after which an ONLINE sensor is considered TIMEOUT.
    /// </summary>
    public int HeartbeatTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Interval, in seconds, between background evaluations of sensor timeouts.
    /// </summary>
    public int TimeoutScanIntervalSeconds { get; set; } = 15;

    /// <summary>
    /// When true, a disconnection alert is generated in Alert Management for each sensor that times out.
    /// </summary>
    public bool RaiseDisconnectionAlerts { get; set; } = true;

    public TimeSpan HeartbeatTimeout => TimeSpan.FromSeconds(Math.Max(1, HeartbeatTimeoutSeconds));

    public TimeSpan TimeoutScanInterval => TimeSpan.FromSeconds(Math.Max(1, TimeoutScanIntervalSeconds));
}
