using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Domain.Services;

namespace ResQ.API.Incident_Management.Interfaces.ACL;

public class IncidentContextFacade(IIncidentCommandService incidentCommandService) : IIncidentContextFacade
{
    public async Task<Guid> CreateIncident(Guid organizationId, Guid zoneId, string type, string level)
    {
        var command = new CreateIncidentCommand(organizationId, zoneId, type, level);

        var incident = await incidentCommandService.Handle(command);

        return incident?.Id.Value ?? Guid.Empty;
    }
}