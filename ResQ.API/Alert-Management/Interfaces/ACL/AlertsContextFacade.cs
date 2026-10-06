using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Services;

namespace ResQ.API.Alert_Management.Interfaces.ACL;

/// <summary>
/// Implementation of the Anti-Corruption Layer facade for Alert Management.
/// </summary>
public class AlertsContextFacade(IAlertCommandService alertCommandService) : IAlertsContextFacade
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
            []);

        var alert = await alertCommandService.Handle(command);

        return alert?.Id ?? Guid.Empty;
    }
}
