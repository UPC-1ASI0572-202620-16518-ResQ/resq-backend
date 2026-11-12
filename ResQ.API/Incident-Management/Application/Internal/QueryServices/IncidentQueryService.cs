using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.Queries;
using ResQ.API.Incident_Management.Domain.Repositories;
using ResQ.API.Incident_Management.Domain.Services;

namespace ResQ.API.Incident_Management.Application.Internal.QueryServices;

public class IncidentQueryService(
    IIncidentRepository incidentRepository)
    : IIncidentQueryService
{
    public async Task<Incident?> Handle(GetIncidentByIdQuery query)
    {
        return await incidentRepository.FindByIncidentIdAsync(query.OrganizationId, query.IncidentId);
    }


    public async Task<IEnumerable<Incident>> Handle(GetHistoricalIncidentsQuery query)
    {
        return await incidentRepository.FindAllAsync(query.OrganizationId, query.ZoneId);
    }
}
