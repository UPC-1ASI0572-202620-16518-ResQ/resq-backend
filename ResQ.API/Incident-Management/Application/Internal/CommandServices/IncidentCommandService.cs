using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Domain.Model.ValueObjects;
using ResQ.API.Incident_Management.Domain.Repositories;
using ResQ.API.Incident_Management.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Incident_Management.Application.Internal.CommandServices;

public class IncidentCommandService(
    IIncidentRepository incidentRepository,
    IBuildingsContextFacade buildingsContextFacade,
    IUnitOfWork unitOfWork)
    : IIncidentCommandService
{
    public async Task<Incident?> Handle(CreateIncidentCommand command)
    {
        var zoneId = new ZoneId(command.ZoneId);

        // The zone is an external reference validated through the Building Management ACL
        if (!await buildingsContextFacade.ZoneExistsAsync(command.OrganizationId, zoneId.Value))
            throw new InvalidOperationException("The specified zone does not exist in the organization.");

        var riskType = ParseRiskType(command.Type);
        var riskLevel = ParseRiskLevel(command.Level);

        var incident = Incident.Create(zoneId, riskType, riskLevel);

        await incidentRepository.AddAsync(incident);
        await unitOfWork.CompleteAsync();

        return incident;
    }

    public async Task<Incident?> Handle(AssignIncidentCommand command)
    {
        var incident = await incidentRepository.FindByIncidentIdAsync(command.IncidentId);

        if (incident == null) throw new KeyNotFoundException("Incident not found.");

        var attendantId = new AttendantId(command.AttendantId);

        incident.AssignAttendant(attendantId);

        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();

        return incident;
    }
    
    public async Task<Incident?> Handle(ChangeIncidentStatusCommand command)
    {
        var incident = await incidentRepository.FindByIncidentIdAsync(command.IncidentId.Value);

        if (incident == null)
        {
            throw new KeyNotFoundException("Incident not found.");
        }

        incident.ChangeStatus(command.Status);

        incidentRepository.Update(incident);

        await unitOfWork.CompleteAsync();

        return incident;
    }

    public async Task<Incident?> Handle(ResolveIncidentCommand command)
    {
        var incident = await incidentRepository.FindByIncidentIdAsync(command.IncidentId);

        if (incident == null)
            throw new KeyNotFoundException("Incident not found.");

        incident.ResolveIncident(command.ResolutionNotes);

        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();

        return incident;
    }

    private static ERiskType ParseRiskType(string type)
    {
        if (!Enum.TryParse<ERiskType>(type, true, out var riskType))
        {
            throw new ArgumentException("Invalid risk type.", nameof(type));
        }

        return riskType;
    }

    private static ERiskLevel ParseRiskLevel(string level)
    {
        if (!Enum.TryParse<ERiskLevel>(level, true, out var riskLevel))
        {
            throw new ArgumentException("Invalid risk level.", nameof(level));
        }

        return riskLevel;
    }
}