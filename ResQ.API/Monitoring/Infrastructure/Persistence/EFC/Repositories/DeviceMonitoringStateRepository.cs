using Microsoft.EntityFrameworkCore;
using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Monitoring.Infrastructure.Persistence.EFC.Repositories;

public class DeviceMonitoringStateRepository(AppDbContext context) : BaseRepository<DeviceMonitoringState>(context), IDeviceMonitoringStateRepository
{
    public async Task<DeviceMonitoringState?> FindByDeviceIdAsync(Guid organizationId, Guid deviceId)
    {
        return await Context.Set<DeviceMonitoringState>()
            .FirstOrDefaultAsync(state =>
                state.OrganizationId == organizationId &&
                state.DeviceId == deviceId);
    }

    public async Task<IEnumerable<DeviceMonitoringState>> FindAllAsync(Guid organizationId)
    {
        return await Context.Set<DeviceMonitoringState>()
            .Where(state => state.OrganizationId == organizationId)
            .ToListAsync();
    }
}