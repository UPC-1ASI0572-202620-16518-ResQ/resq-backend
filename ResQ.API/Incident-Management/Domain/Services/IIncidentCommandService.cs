using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.Commands;

namespace ResQ.API.Incident_Management.Domain.Services;

public interface IIncidentCommandService
{
    Task<Incident?> Handle(CreateIncidentCommand command);

    Task<Incident?> Handle(AssignIncidentCommand command);
    
    Task<Incident?> Handle(ChangeIncidentStatusCommand command);

    Task<Incident?> Handle(ResolveIncidentCommand command);
}