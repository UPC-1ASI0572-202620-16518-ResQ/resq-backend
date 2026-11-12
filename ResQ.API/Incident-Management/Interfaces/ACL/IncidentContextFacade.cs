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

    public async Task<Guid> CreateIncidentForDetection(
        Guid organizationId, Guid zoneId, string type, string level, Guid riskDetectionId, Guid? alertId)
    {
        var incident = await incidentCommandService.Handle(new CreateIncidentCommand(
            organizationId, zoneId, type, level, riskDetectionId, alertId));
        return incident?.Id.Value ?? Guid.Empty;
    }
}
