using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class ResponsePolicyCommandFromResourceAssembler
{
    public static ConfigureResponsePolicyCommand ToCommandFromResource(Guid organizationId, ConfigureResponsePolicyResource resource)
    {
        return new ConfigureResponsePolicyCommand(organizationId, resource.RiskTypeCode, ToDefinitions(resource.Actions));
    }

    public static UpdateResponsePolicyCommand ToCommandFromResource(Guid organizationId, Guid policyId, UpdateResponsePolicyResource resource)
    {
        return new UpdateResponsePolicyCommand(organizationId, policyId, resource.RiskTypeCode, ToDefinitions(resource.Actions));
    }

    public static ChangeResponsePolicyStatusCommand ToCommandFromResource(Guid organizationId, Guid policyId,
        ChangeResponsePolicyStatusResource resource)
    {
        return new ChangeResponsePolicyStatusCommand(organizationId, policyId, resource.Active);
    }

    private static List<ResponseActionDefinition> ToDefinitions(IEnumerable<ResponseActionDataResource>? actions)
    {
        return (actions ?? [])
            .Select(action => new ResponseActionDefinition(
                action.ActionCode,
                action.TargetDeviceId,
                action.TargetCapabilityCode,
                EnumCode.Parse<EAuthorizationMode>(action.AuthorizationMode, "authorizationMode"),
                action.Critical))
            .ToList();
    }
}
