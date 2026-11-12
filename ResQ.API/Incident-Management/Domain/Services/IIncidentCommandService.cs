using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.Commands;

namespace ResQ.API.Incident_Management.Domain.Services;

public interface IIncidentCommandService
{
    Task<Incident?> Handle(CreateIncidentCommand command);

    Task<Incident?> Handle(AssignIncidentCommand command);
    
    Task<Incident?> Handle(ChangeIncidentStatusCommand command);

    Task<Incident?> Handle(ResolveIncidentCommand command);

    Task<Incident?> Handle(AssignIncidentToCurrentUserCommand command);

    Task<IncidentHumanAction> Handle(RecordIncidentHumanActionCommand command);

    Task<IncidentSafetyVerification> Handle(VerifyIncidentSafetyCommand command);

    Task<Incident?> Handle(ResolveIncidentAsCurrentUserCommand command);

    Task<Incident?> Handle(ChangeIncidentStatusAsCurrentUserCommand command);
}
