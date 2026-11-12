using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Monitoring.Domain.Repositories;

public interface IZoneMonitoringStateRepository : IBaseRepository<ZoneMonitoringState>
{
    Task<ZoneMonitoringState?> FindByZoneIdAsync(Guid organizationId, Guid zoneId);

    Task<IEnumerable<ZoneMonitoringState>> FindByBuildingIdAsync(Guid organizationId, Guid buildingId);
}