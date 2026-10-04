using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Alert_Management.Infrastructure.Persistence.EFC.Repositories;

public class AlertRepository(AppDbContext context) : BaseRepository<Alert>(context), IAlertRepository
{
    public async Task<Alert?> FindByIdAndOrganizationIdAsync(Guid alertId, Guid organizationId)
    {
        return await Context.Set<Alert>().Include(alert => alert.Deliveries)
            .FirstOrDefaultAsync(alert =>
                alert.Id == alertId &&
                alert.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<Alert>> FindAllAsync(Guid organizationId, Guid? buildingId, Guid? zoneId, string? riskTypeCode,
        DateTimeOffset? from, DateTimeOffset? to)
    {
        var query = Context.Set<Alert>().Include(alert => alert.Deliveries)
            .Where(alert => alert.OrganizationId == organizationId);

        if (buildingId.HasValue)
            query = query.Where(alert => alert.Context.BuildingId == buildingId.Value);

        if (zoneId.HasValue)
            query = query.Where(alert => alert.Context.ZoneId == zoneId.Value);

        if (riskTypeCode is not null)
            query = query.Where(alert => alert.Context.RiskTypeCode == riskTypeCode);

        if (from.HasValue)
            query = query.Where(alert => alert.GeneratedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(alert => alert.GeneratedAt <= to.Value);

        // Most recent alerts first
        return await query
            .OrderByDescending(alert => alert.GeneratedAt)
            .ToListAsync();
    }
}
