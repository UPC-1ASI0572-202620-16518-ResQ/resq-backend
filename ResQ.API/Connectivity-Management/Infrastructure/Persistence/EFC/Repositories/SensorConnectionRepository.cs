using Microsoft.EntityFrameworkCore;
using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Connectivity_Management.Infrastructure.Persistence.EFC.Repositories;

public class SensorConnectionRepository(AppDbContext context)
    : BaseRepository<SensorConnection>(context), ISensorConnectionRepository
{
    public async Task<SensorConnection?> FindBySensorIdAsync(Guid organizationId, Guid sensorId)
    {
        return await Context.Set<SensorConnection>()
            .FirstOrDefaultAsync(connection =>
                connection.OrganizationId == organizationId &&
                connection.SensorId == sensorId);
    }

    public async Task<IEnumerable<SensorConnection>> FindDisconnectedAsync(Guid organizationId, DateTimeOffset heartbeatLimit)
    {
        return await Context.Set<SensorConnection>()
            .Where(connection =>
                connection.OrganizationId == organizationId &&
                (connection.Status != EConnectionStatus.Online || connection.LastHeartbeat < heartbeatLimit))
            .OrderBy(connection => connection.LastHeartbeat)
            .ToListAsync();
    }

    public async Task<IEnumerable<SensorConnection>> FindOnlineWithLastHeartbeatBeforeAsync(DateTimeOffset time)
    {
        return await Context.Set<SensorConnection>()
            .Where(connection =>
                connection.Status == EConnectionStatus.Online &&
                connection.LastHeartbeat < time)
            .ToListAsync();
    }
}
