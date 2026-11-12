using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.API.Alert_Management.Interfaces.ACL;

/// <summary>
/// Implementation of the Anti-Corruption Layer facade for Alert Management.
/// </summary>
public class AlertsContextFacade(IAlertCommandService alertCommandService, AppDbContext context) : IAlertsContextFacade
{
    public async Task<Guid> GenerateAlertAsync(
        Guid organizationId,
        string riskDetectionId,
        string riskTypeCode,
        string severityCode,
        Guid? buildingId,
        Guid? zoneId,
        DateTimeOffset detectedAt,
        IEnumerable<AlertRecipient> recipients)
    {
        var policy = await context.Set<ResponsePolicy>()
            .Where(candidate => candidate.OrganizationId == organizationId && candidate.Active &&
                                candidate.RiskTypeCode == riskTypeCode.Trim().ToUpper() &&
                                candidate.SeverityCode == severityCode.Trim().ToUpper() &&
                                (candidate.ZoneId == zoneId || candidate.ZoneId == null))
            .OrderByDescending(candidate => candidate.ZoneId.HasValue)
            .FirstOrDefaultAsync();
        var responseActions = policy is null
            ? []
            : JsonSerializer.Deserialize<List<ResponsePolicyAction>>(policy.ActionsJson) ?? [];

        var command = new GenerateAlertCommand(
            organizationId,
            riskDetectionId,
            riskTypeCode,
            severityCode,
            buildingId,
            zoneId,
            detectedAt,
            recipients
                .Select(recipient => new NotificationRecipient(recipient.RecipientUserId, recipient.Channel, recipient.Destination))
                .ToList(),
            responseActions.Select(action => new ResponseActionSnapshot(
                action.ActionCode, action.TargetDeviceId, action.TargetCapabilityCode,
                Enum.TryParse<EAuthorizationMode>(action.AuthorizationMode, true, out var mode)
                    ? mode : EAuthorizationMode.HumanRequired,
                action.Critical)).ToList());

        var alert = await alertCommandService.Handle(command);

        return alert?.Id ?? Guid.Empty;
    }
}

public record ResponsePolicyAction(
    string ActionCode, Guid TargetDeviceId, string TargetCapabilityCode, string AuthorizationMode, bool Critical);
