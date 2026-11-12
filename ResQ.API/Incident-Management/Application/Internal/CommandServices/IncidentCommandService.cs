using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.Commands;
using ResQ.API.Incident_Management.Domain.Model.ValueObjects;
using ResQ.API.Incident_Management.Domain.Repositories;
using ResQ.API.Incident_Management.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;
using ResQ.API.Monitoring.Domain.Model.Queries;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;
using ResQ.API.Monitoring.Domain.Services;
using ResQ.API.Risk_Detection.Domain.Model.Queries;
using ResQ.API.Risk_Detection.Domain.Services;

namespace ResQ.API.Incident_Management.Application.Internal.CommandServices;

public class IncidentCommandService(
    IIncidentRepository incidentRepository,
    IBuildingsContextFacade buildingsContextFacade,
    IRiskDetectionQueryService riskDetectionQueryService,
    IDetectionRuleQueryService detectionRuleQueryService,
    IMonitoringQueryService monitoringQueryService,
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

        if (command.RiskDetectionId.HasValue)
        {
            var existing = await incidentRepository.FindByRiskDetectionIdAsync(
                command.OrganizationId, command.RiskDetectionId.Value);
            if (existing is not null) return existing;
        }

        var incident = Incident.Create(
            command.OrganizationId, zoneId, riskType, riskLevel, command.RiskDetectionId, command.AlertId);

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

    public async Task<Incident?> Handle(AssignIncidentToCurrentUserCommand command)
    {
        var incident = await FindScoped(command.OrganizationId, command.IncidentId);
        incident.AssignCurrentUser(command.UserId);
        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();
        return incident;
    }

    public async Task<IncidentHumanAction> Handle(RecordIncidentHumanActionCommand command)
    {
        var incident = await FindScoped(command.OrganizationId, command.IncidentId);
        var action = incident.AddHumanAction(command.UserId, command.ActionCode, command.Notes);
        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();
        return action;
    }

    public async Task<IncidentSafetyVerification> Handle(VerifyIncidentSafetyCommand command)
    {
        var incident = await FindScoped(command.OrganizationId, command.IncidentId);
        if (!incident.RiskDetectionId.HasValue)
            return await RecordUnsafe(incident, command.UserId, "Incident has no linked risk detection.");

        var detection = await riskDetectionQueryService.Handle(
            new GetRiskDetectionEvidenceQuery(command.OrganizationId, incident.RiskDetectionId.Value));
        if (detection is null)
            return await RecordUnsafe(incident, command.UserId, "Linked risk detection was not found.");

        var rule = await detectionRuleQueryService.Handle(
            new GetDetectionRuleByIdQuery(command.OrganizationId, detection.RuleId));
        if (rule is null)
            return await RecordUnsafe(incident, command.UserId, "Linked detection rule was not found.");

        var sourceEvidence = detection.Evidence.OrderByDescending(evidence => evidence.MeasuredAt).FirstOrDefault();
        if (sourceEvidence is null)
            return await RecordUnsafe(incident, command.UserId, "Risk detection has no source evidence.");

        var availability = await monitoringQueryService.GetEffectiveDeviceAvailabilityAsync(
            command.OrganizationId, sourceEvidence.DeviceId);
        if (availability != EDeviceAvailability.Available)
            return await RecordUnsafe(incident, command.UserId, "Source device is not currently available.", sourceEvidence.DeviceId);

        var current = (await monitoringQueryService.Handle(new GetCurrentDeviceMeasurementsQuery(
                command.OrganizationId, sourceEvidence.DeviceId, DateTimeOffset.UtcNow)))
            .Where(measurement => rule.Condition.Supports(measurement.VariableType))
            .OrderByDescending(measurement => measurement.MeasuredAt)
            .FirstOrDefault();

        if (current is null)
            return await RecordUnsafe(incident, command.UserId, "No fresh current measurement exists for the linked rule.", sourceEvidence.DeviceId);

        var safe = !rule.Condition.Matches(current.VariableType, current.MeasurementValue.Value);
        var snapshot = $"{rule.Condition.VariableType}|{rule.Condition.Operator}|{rule.Condition.Threshold}";
        var reason = safe
            ? "Fresh server-side evidence is within the safe side of the linked rule."
            : "The latest fresh measurement still satisfies the linked risk rule.";

        var verification = incident.RecordSafetyVerification(
            command.UserId, safe, reason, current.DeviceId, current.Id,
            current.MeasurementValue.Value, current.MeasuredAt, snapshot);
        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();
        return verification;
    }

    public async Task<Incident?> Handle(ResolveIncidentAsCurrentUserCommand command)
    {
        var incident = await FindScoped(command.OrganizationId, command.IncidentId);
        if (incident.AssignedUserId != command.UserId)
            throw new InvalidOperationException("Only the assigned user can resolve this incident.");
        incident.ResolveIncident(command.ResolutionNotes, command.UserId);
        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();
        return incident;
    }

    public async Task<Incident?> Handle(ChangeIncidentStatusAsCurrentUserCommand command)
    {
        var incident = await FindScoped(command.OrganizationId, command.IncidentId);
        if (command.Status == EIncidentStatus.Closed && incident.AssignedUserId != command.UserId)
            throw new InvalidOperationException("Only the assigned user can close this incident.");
        incident.ChangeStatus(command.Status, command.UserId);
        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();
        return incident;
    }

    private async Task<Incident> FindScoped(Guid organizationId, Guid incidentId)
    {
        return await incidentRepository.FindByIncidentIdAsync(organizationId, incidentId)
               ?? throw new KeyNotFoundException("Incident not found.");
    }

    private async Task<IncidentSafetyVerification> RecordUnsafe(
        Incident incident, int userId, string reason, Guid? deviceId = null)
    {
        var verification = incident.RecordSafetyVerification(
            userId, false, reason, deviceId, null, null, null, null);
        incidentRepository.Update(incident);
        await unitOfWork.CompleteAsync();
        return verification;
    }

    private static ERiskType ParseRiskType(string type)
    {
        var normalized = type.Replace("_", string.Empty).Replace("-", string.Empty).Trim();
        return normalized.ToUpperInvariant() switch
        {
            "FIRE" or "SMOKE" or "HEAT" => ERiskType.Fire,
            "GASLEAK" or "GAS" => ERiskType.GasLeak,
            "EARTHQUAKE" or "SEISMIC" => ERiskType.Earthquake,
            _ => ERiskType.Unknown
        };
    }

    private static ERiskLevel ParseRiskLevel(string level)
    {
        var normalized = level.Trim().ToUpperInvariant();
        return normalized switch
        {
            "CRITICAL" or "CRITICO" => ERiskLevel.Critical,
            "HIGH" => ERiskLevel.High,
            "MEDIUM" or "WARNING" => ERiskLevel.Medium,
            _ => ERiskLevel.Low
        };
    }
}
