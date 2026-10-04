using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class DecideResponseAuthorizationCommandFromResourceAssembler
{
    public static DecideResponseAuthorizationCommand ToCommandFromResource(Guid organizationId, Guid responseExecutionId,
        string decidedByUserId, DecideResponseAuthorizationResource resource)
    {
        var decision = EnumCode.Parse<EAuthorizationDecision>(resource.Decision, "decision");

        return new DecideResponseAuthorizationCommand(organizationId, responseExecutionId, decision, decidedByUserId);
    }
}
