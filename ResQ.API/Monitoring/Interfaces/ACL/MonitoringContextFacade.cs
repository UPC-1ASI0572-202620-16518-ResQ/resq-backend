using ResQ.API.Monitoring.Domain.Model.Queries;
using ResQ.API.Monitoring.Domain.Services;

namespace ResQ.API.Monitoring.Interfaces.ACL;

/// <summary>
/// Implementation of the Monitoring Anti-Corruption Layer facade.
/// </summary>
public class MonitoringContextFacade(IMonitoringQueryService monitoringQueryService) : IMonitoringContextFacade
{
    public async Task<string?> GetZoneConditionAsync(Guid organizationId, Guid zoneId)
    {
        var state = await monitoringQueryService.Handle(new GetZoneMonitoringStatusQuery(organizationId, zoneId));
        return state?.ConditionCode;
    }
}