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
        return await incidentRepository.FindByIncidentIdAsync(query.IncidentId);
    }


    public async Task<IEnumerable<Incident>> Handle(GetHistoricalIncidentsQuery query)
    {
        if (query.ZoneId.HasValue)
        {
            return await incidentRepository.FindAllByZoneIdAsync(query.ZoneId.Value);
        }

        return await incidentRepository.ListAsync();
    }
}