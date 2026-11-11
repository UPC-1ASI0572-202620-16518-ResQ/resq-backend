namespace ResQ.API.Connectivity_Management.Domain.Model.Commands;

/// <summary>
/// Registers a new heartbeat of a sensor.
/// </summary>
public record RegisterHeartbeatCommand(Guid OrganizationId, Guid SensorId, DateTimeOffset Timestamp, int? SignalStrength);

/// <summary>
/// Forces a sensor to OFFLINE.
/// </summary>
public record MarkSensorOfflineCommand(Guid OrganizationId, Guid SensorId);

/// <summary>
/// Internal command: marks as TIMEOUT every ONLINE sensor whose last heartbeat is older than the configured threshold.
/// </summary>
public record EvaluateSensorTimeoutsCommand(DateTimeOffset EvaluatedAt);
