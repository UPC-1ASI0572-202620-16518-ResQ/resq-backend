using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Connectivity_Management.Domain.Model.Entities;

/// <summary>
/// Single heartbeat sent by a sensor to notify that it is still alive.
/// Belongs to the SensorConnection aggregate.
/// </summary>
public class HeartbeatRecord
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Date and time when the heartbeat was received.
    /// </summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>
    /// Signal strength reported with the heartbeat, if any.
    /// </summary>
    public SignalStrength? SignalStrength { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected HeartbeatRecord()
    {
    }

    internal HeartbeatRecord(DateTimeOffset timestamp, SignalStrength? signalStrength)
    {
        Id = Guid.NewGuid();
        Timestamp = timestamp;
        SignalStrength = signalStrength;
    }
}
