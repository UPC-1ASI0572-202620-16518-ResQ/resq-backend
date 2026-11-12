using Microsoft.EntityFrameworkCore;
using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.ValueObjects;
using ResQ.API.Incident_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Incident_Management.Infrastructure.Persistence.EFC.Repositories;

public class IncidentRepository(AppDbContext context) : BaseRepository<Incident>(context), IIncidentRepository
{
    public async Task<Incident?> FindByIncidentIdAsync(Guid incidentId)
    {
        var id = new IncidentId(incidentId);

        return await WithDetails().FirstOrDefaultAsync(incident => incident.Id == id);
    }

    public async Task<Incident?> FindByIncidentIdAsync(Guid organizationId, Guid incidentId)
    {
        var id = new IncidentId(incidentId);
        return await WithDetails().FirstOrDefaultAsync(incident =>
            incident.Id == id && incident.OrganizationId == organizationId);
    }

    public async Task<Incident?> FindByRiskDetectionIdAsync(Guid organizationId, Guid riskDetectionId)
    {
        return await WithDetails().FirstOrDefaultAsync(incident =>
            incident.OrganizationId == organizationId && incident.RiskDetectionId == riskDetectionId);
    }

    public async Task<IEnumerable<Incident>> FindAllByZoneIdAsync(Guid zoneId)
    {
        var id = new ZoneId(zoneId);

        return await WithDetails().Where(incident => incident.ZoneId == id).ToListAsync();
    }

    public async Task<IEnumerable<Incident>> FindAllAsync(Guid organizationId, Guid? zoneId = null)
    {
        var query = WithDetails().Where(incident => incident.OrganizationId == organizationId);
        if (zoneId.HasValue) query = query.Where(incident => incident.ZoneId == new ZoneId(zoneId.Value));
        return await query.OrderByDescending(incident => incident.CreatedAt).ToListAsync();
    }

    private IQueryable<Incident> WithDetails() => Context.Set<Incident>()
        .Include(incident => incident.HumanActions)
        .Include(incident => incident.SafetyVerifications);
}
