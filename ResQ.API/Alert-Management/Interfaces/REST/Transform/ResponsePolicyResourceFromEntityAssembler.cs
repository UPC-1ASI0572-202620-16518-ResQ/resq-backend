using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class ResponsePolicyResourceFromEntityAssembler
{
    public static ResponsePolicyResource ToResourceFromEntity(ResponsePolicy entity)
    {
        var actions = entity.Actions
            .Select(action => new ResponseActionResource(
                action.Id,
                action.ActionCode,
                action.TargetDeviceId,
                action.TargetCapabilityCode,
                EnumCode.ToCode(action.AuthorizationMode),
                action.Critical))
            .ToList();

        return new ResponsePolicyResource(
            entity.Id,
            entity.OrganizationId,
            entity.RiskTypeCode,
            EnumCode.ToCode(entity.Status),
            actions,
            entity.Version);
    }
}
