using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Application.Internal.CommandServices;

public class ResponseExecutionCommandService(
    IResponseExecutionRepository responseExecutionRepository,
    IUnitOfWork unitOfWork)
    : IResponseExecutionCommandService
{
    public async Task<ResponseExecution?> Handle(DecideResponseAuthorizationCommand command)
    {
        var execution = await FindExecutionAsync(command.OrganizationId, command.ResponseExecutionId);

        execution.DecideAuthorization(command.Decision, command.DecidedByUserId);

        await unitOfWork.CompleteAsync();

        return execution;
    }

    public async Task<ResponseExecution?> Handle(RecordResponseExecutionResultCommand command)
    {
        var execution = await FindExecutionAsync(command.OrganizationId, command.ResponseExecutionId);

        execution.RecordResult(command.Successful, command.ResultCode, command.Message);

        await unitOfWork.CompleteAsync();

        return execution;
    }

    private async Task<ResponseExecution> FindExecutionAsync(Guid organizationId, Guid responseExecutionId)
    {
        return await responseExecutionRepository.FindByIdAndOrganizationIdAsync(responseExecutionId, organizationId)
               ?? throw new KeyNotFoundException("Response execution not found.");
    }
}
