using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class ResponseExecutionResourceFromEntityAssembler
{
    public static ResponseExecutionResource ToResourceFromEntity(ResponseExecution entity)
    {
        var action = new ResponseActionSnapshotResource(
            entity.Action.ActionId,
            entity.Action.ActionCode,
            entity.Action.TargetDeviceId,
            entity.Action.TargetCapabilityCode,
            EnumCode.ToCode(entity.Action.AuthorizationMode),
            entity.Action.Critical);

        var authorization = entity.Authorization is null
            ? null
            : new ResponseAuthorizationResource(
                entity.Authorization.AuthorizationId,
                EnumCode.ToCode(entity.Authorization.Decision),
                entity.Authorization.DecidedByUserId,
                entity.Authorization.DecidedAt);

        return new ResponseExecutionResource(
            entity.Id,
            entity.OrganizationId,
            entity.AlertId,
            entity.RiskDetectionId,
            action,
            EnumCode.ToCode(entity.Status),
            entity.RequestedAt,
            authorization);
    }
}
