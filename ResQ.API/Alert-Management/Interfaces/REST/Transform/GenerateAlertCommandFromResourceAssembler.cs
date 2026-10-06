using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class GenerateAlertCommandFromResourceAssembler
{
    public static GenerateAlertCommand ToCommandFromResource(Guid organizationId, GenerateAlertResource resource)
    {
        var recipients = (resource.Recipients ?? [])
            .Select(recipient => new NotificationRecipient(recipient.RecipientUserId, recipient.Channel, recipient.Destination))
            .ToList();

        var responseActions = (resource.ResponseActions ?? [])
            .Select(action => new ResponseActionSnapshot(
                action.ActionCode,
                action.TargetDeviceId,
                action.TargetCapabilityCode,
                EnumCode.Parse<EAuthorizationMode>(action.AuthorizationMode, "authorizationMode"),
                action.Critical))
            .ToList();

        return new GenerateAlertCommand(
            organizationId,
            resource.RiskDetectionId,
            resource.RiskTypeCode,
            resource.SeverityCode,
            resource.BuildingId,
            resource.ZoneId,
            resource.DetectedAt,
            recipients,
            responseActions);
    }
}
