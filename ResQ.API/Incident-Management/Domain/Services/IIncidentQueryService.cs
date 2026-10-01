using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.Queries;

namespace ResQ.API.Incident_Management.Domain.Services;

public interface IIncidentQueryService
{
    Task<Incident?> Handle(GetIncidentByIdQuery query);

    Task<IEnumerable<Incident>> Handle(GetHistoricalIncidentsQuery query);
}