using ResQ.API.Connectivity_Management.Domain.Model.Entities;
using ResQ.API.Connectivity_Management.Domain.Model.Events;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Connectivity_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that represents the network status and current connectivity session of a sensor.
/// The sensor is a device registered in Device Management; SensorId is that device identifier.
/// </summary>
public class SensorConnection
{
    private readonly List<HeartbeatRecord> _heartbeats = new();
    private readonly List<IConnectivityDomainEvent> _domainEvents = new();

    /// <summary>
    /// Unique identifier of the connection.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Organization that owns the sensor.
    /// </summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Identifier of the sensor (device id in Device Management).
    /// </summary>
    public Guid SensorId { get; private set; }

    /// <summary>
    /// Current network status.
    /// </summary>
    public EConnectionStatus Status { get; private set; }

    /// <summary>
    /// Date and time of the last heartbeat received.
    /// </summary>
    public DateTimeOffset LastHeartbeat { get; private set; }

    /// <summary>
    /// Signal strength reported with the last heartbeat, if any.
    /// </summary>
    public SignalStrength? SignalStrength { get; private set; }

    /// <summary>
    /// Date and time of the last status change.
    /// </summary>
    public DateTimeOffset StatusChangedAt { get; private set; }

    /// <summary>
    /// Heartbeats registered for this connection.
    /// </summary>
    public IReadOnlyCollection<HeartbeatRecord> Heartbeats => _heartbeats.AsReadOnly();

    /// <summary>
    /// Domain events raised since the aggregate was loaded. Not persisted.
    /// </summary>
    public IReadOnlyCollection<IConnectivityDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected SensorConnection()
    {
    }

    /// <summary>
    /// Opens the connection of a sensor with its first heartbeat.
    /// </summary>
    public static SensorConnection Open(Guid organizationId, Guid sensorId, DateTimeOffset timestamp, SignalStrength? signalStrength)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        if (sensorId == Guid.Empty)
            throw new ArgumentException("Sensor id is required.");

        var connection = new SensorConnection
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            SensorId = sensorId,
            Status = EConnectionStatus.Online,
            LastHeartbeat = timestamp,
            StatusChangedAt = timestamp
        };

        connection.RegisterHeartbeat(timestamp, signalStrength);

        return connection;
    }

    /// <summary>
    /// Registers a heartbeat. An OFFLINE or TIMEOUT sensor goes back ONLINE.
    /// </summary>
    public void RegisterHeartbeat(DateTimeOffset timestamp, SignalStrength? signalStrength)
    {
        if (timestamp == default)
            throw new ArgumentException("Heartbeat timestamp is required.");

        var previousStatus = Status;

        _heartbeats.Add(new HeartbeatRecord(timestamp, signalStrength));

        if (timestamp > LastHeartbeat) LastHeartbeat = timestamp;
        SignalStrength = signalStrength;

        if (Status != EConnectionStatus.Online)
        {
            Status = EConnectionStatus.Online;
            StatusChangedAt = timestamp;
        }

        _domainEvents.Add(new HeartbeatReceivedEvent(SensorId, previousStatus, timestamp));
    }

    /// <summary>
    /// True when the time elapsed since the last heartbeat exceeds the allowed limit.
    /// </summary>
    public bool IsTimeoutExceeded(DateTimeOffset now, TimeSpan timeout)
    {
        return now - LastHeartbeat > timeout;
    }

    /// <summary>
    /// Status as seen at <paramref name="now"/>: an ONLINE sensor whose heartbeat expired is reported as TIMEOUT
    /// even before the background evaluation persists that change.
    /// </summary>
    public EConnectionStatus GetEffectiveStatus(DateTimeOffset now, TimeSpan timeout)
    {
        return Status == EConnectionStatus.Online && IsTimeoutExceeded(now, timeout)
            ? EConnectionStatus.Timeout
            : Status;
    }

    /// <summary>
    /// Marks the sensor as TIMEOUT when its heartbeat expired.
    /// </summary>
    /// <returns>True when the status changed.</returns>
    public bool MarkAsTimedOut(DateTimeOffset now, TimeSpan timeout)
    {
        if (Status != EConnectionStatus.Online || !IsTimeoutExceeded(now, timeout)) return false;

        ChangeToDisconnected(EConnectionStatus.Timeout, now);
        return true;
    }

    /// <summary>
    /// Forces the sensor to OFFLINE (e.g. the device was decommissioned or reported a shutdown).
    /// </summary>
    /// <returns>True when the status changed.</returns>
    public bool MarkAsOffline(DateTimeOffset now)
    {
        if (Status == EConnectionStatus.Offline) return false;

        ChangeToDisconnected(EConnectionStatus.Offline, now);
        return true;
    }

    private void ChangeToDisconnected(EConnectionStatus status, DateTimeOffset now)
    {
        Status = status;
        StatusChangedAt = now;
        _domainEvents.Add(new ConnectionLostEvent(SensorId, status, LastHeartbeat, now));
    }
}
