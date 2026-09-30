using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Incident_Management.Domain.Repositories;

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<Incident?> FindByIncidentIdAsync(Guid incidentId);
    
    Task<IEnumerable<Incident>> FindAllByZoneIdAsync(Guid zoneId);
}