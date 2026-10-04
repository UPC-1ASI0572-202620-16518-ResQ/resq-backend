using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Interfaces.REST.Resources;

namespace ResQ.API.Alert_Management.Interfaces.REST.Transform;

public static class RecordResponseExecutionResultCommandFromResourceAssembler
{
    public static RecordResponseExecutionResultCommand ToCommandFromResource(Guid organizationId, Guid responseExecutionId,
        RecordResponseExecutionResultResource resource)
    {
        return new RecordResponseExecutionResultCommand(
            organizationId, responseExecutionId, resource.Successful, resource.ResultCode, resource.Message);
    }
}
