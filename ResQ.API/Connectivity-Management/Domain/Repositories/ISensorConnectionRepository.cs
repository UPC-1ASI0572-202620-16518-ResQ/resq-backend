using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Connectivity_Management.Domain.Repositories;

public interface ISensorConnectionRepository : IBaseRepository<SensorConnection>
{
    /// <summary>
    /// Finds the connection of a sensor (heartbeat history is not loaded).
    /// </summary>
    Task<SensorConnection?> FindBySensorIdAsync(Guid organizationId, Guid sensorId);

    /// <summary>
    /// Finds the sensors of the organization that are OFFLINE/TIMEOUT or whose last heartbeat is older than the limit.
    /// </summary>
    Task<IEnumerable<SensorConnection>> FindDisconnectedAsync(Guid organizationId, DateTimeOffset heartbeatLimit);

    /// <summary>
    /// Finds, across organizations, the ONLINE sensors whose last heartbeat was before the given time.
    /// </summary>
    Task<IEnumerable<SensorConnection>> FindOnlineWithLastHeartbeatBeforeAsync(DateTimeOffset time);
}
