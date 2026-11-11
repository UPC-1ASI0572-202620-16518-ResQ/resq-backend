namespace ResQ.API.Connectivity_Management.Domain.Model.Queries;

/// <summary>
/// Retrieves the current network status of a sensor.
/// </summary>
public record GetSensorConnectionStatusQuery(Guid OrganizationId, Guid SensorId);

/// <summary>
/// Retrieves every sensor of the organization that has currently lost its connection (OFFLINE or TIMEOUT).
/// </summary>
public record GetAllOfflineSensorsQuery(Guid OrganizationId);
