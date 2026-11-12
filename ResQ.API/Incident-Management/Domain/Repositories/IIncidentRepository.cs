using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Incident_Management.Domain.Repositories;

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<Incident?> FindByIncidentIdAsync(Guid organizationId, Guid incidentId);

    Task<IEnumerable<Incident>> FindAllByOrganizationIdAsync(Guid organizationId);

    Task<IEnumerable<Incident>> FindAllByZoneIdAsync(Guid organizationId, Guid zoneId);
}
