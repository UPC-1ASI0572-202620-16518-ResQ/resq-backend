using Microsoft.EntityFrameworkCore;
using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.ValueObjects;
using ResQ.API.Incident_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Incident_Management.Infrastructure.Persistence.EFC.Repositories;

public class IncidentRepository(AppDbContext context) : BaseRepository<Incident>(context), IIncidentRepository
{
    public async Task<Incident?> FindByIncidentIdAsync(Guid organizationId, Guid incidentId)
    {
        var id = new IncidentId(incidentId);
        return await Context.Set<Incident>()
            .FirstOrDefaultAsync(incident => incident.Id == id && incident.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<Incident>> FindAllByOrganizationIdAsync(Guid organizationId)
    {
        return await Context.Set<Incident>()
            .Where(incident => incident.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Incident>> FindAllByZoneIdAsync(Guid organizationId, Guid zoneId)
    {
        var id = new ZoneId(zoneId);
        return await Context.Set<Incident>()
            .Where(incident => incident.OrganizationId == organizationId && incident.ZoneId == id)
            .ToListAsync();
    }
}
