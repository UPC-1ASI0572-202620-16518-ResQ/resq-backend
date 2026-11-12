using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Incident_Management.Domain.Repositories;

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<Incident?> FindByIncidentIdAsync(Guid incidentId);

    Task<Incident?> FindByIncidentIdAsync(Guid organizationId, Guid incidentId);

    Task<Incident?> FindByRiskDetectionIdAsync(Guid organizationId, Guid riskDetectionId);
    
    Task<IEnumerable<Incident>> FindAllByZoneIdAsync(Guid zoneId);

    Task<IEnumerable<Incident>> FindAllAsync(Guid organizationId, Guid? zoneId = null);
}
