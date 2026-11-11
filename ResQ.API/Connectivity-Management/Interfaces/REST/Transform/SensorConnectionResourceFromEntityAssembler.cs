using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Interfaces.REST.Resources;

namespace ResQ.API.Connectivity_Management.Interfaces.REST.Transform;

public static class SensorConnectionResourceFromEntityAssembler
{
    /// <summary>
    /// Converts a SensorConnection into its resource, reporting the effective status at the given time
    /// (an ONLINE sensor whose heartbeat expired is shown as TIMEOUT).
    /// </summary>
    public static SensorConnectionResource ToResourceFromEntity(SensorConnection entity, DateTimeOffset now, TimeSpan timeout)
    {
        return new SensorConnectionResource(
            entity.SensorId,
            entity.GetEffectiveStatus(now, timeout).ToString().ToUpperInvariant(),
            entity.LastHeartbeat,
            entity.SignalStrength?.Dbm);
    }
}
