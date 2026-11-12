using Microsoft.EntityFrameworkCore;
using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Monitoring.Infrastructure.Persistence.EFC.Repositories;

public class ZoneMonitoringStateRepository(AppDbContext context) : BaseRepository<ZoneMonitoringState>(context), IZoneMonitoringStateRepository
{
    public async Task<ZoneMonitoringState?> FindByZoneIdAsync(Guid organizationId, Guid zoneId)
    {
        return await Context.Set<ZoneMonitoringState>()
            .FirstOrDefaultAsync(state =>
                state.OrganizationId == organizationId &&
                state.ZoneId == zoneId);
    }

    public async Task<IEnumerable<ZoneMonitoringState>> FindByBuildingIdAsync(Guid organizationId, Guid buildingId)
    {
        return await Context.Set<ZoneMonitoringState>()
            .Where(state =>
                state.OrganizationId == organizationId &&
                state.BuildingId == buildingId)
            .OrderBy(state => state.ZoneId)
            .ToListAsync();
    }
}