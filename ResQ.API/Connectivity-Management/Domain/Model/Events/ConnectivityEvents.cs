using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Connectivity_Management.Domain.Model.Events;

/// <summary>
/// Domain event raised by the SensorConnection aggregate.
/// </summary>
public interface IConnectivityDomainEvent
{
    Guid SensorId { get; }

    DateTimeOffset OccurredAt { get; }
}

/// <summary>
/// Raised every time a heartbeat is registered.
/// </summary>
public sealed record HeartbeatReceivedEvent(
    Guid SensorId,
    EConnectionStatus PreviousStatus,
    DateTimeOffset OccurredAt) : IConnectivityDomainEvent;

/// <summary>
/// Critical event raised when a sensor goes OFFLINE or TIMEOUT.
/// </summary>
public sealed record ConnectionLostEvent(
    Guid SensorId,
    EConnectionStatus NewStatus,
    DateTimeOffset LastHeartbeat,
    DateTimeOffset OccurredAt) : IConnectivityDomainEvent;
