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
        var execution = await responseExecutionRepository.FindByIdAndAlertIdAsync(
                            command.ResponseExecutionId, command.AlertId, command.OrganizationId)
                        ?? throw new KeyNotFoundException("Response execution not found.");

        execution.DecideAuthorization(command.Decision, command.DecidedByUserId);

        await unitOfWork.CompleteAsync();

        return execution;
    }
}
