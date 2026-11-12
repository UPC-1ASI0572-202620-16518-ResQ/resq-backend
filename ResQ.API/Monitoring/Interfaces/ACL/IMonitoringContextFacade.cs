namespace ResQ.API.Monitoring.Interfaces.ACL;

/// <summary>
/// Anti-Corruption Layer exposed by Monitoring to other bounded contexts.
/// </summary>
public interface IMonitoringContextFacade
{
    /// <summary>
    /// Returns the current condition code of a monitored zone, or null when no Monitoring state exists.
    /// </summary>
    Task<string?> GetZoneConditionAsync(Guid organizationId, Guid zoneId);
}