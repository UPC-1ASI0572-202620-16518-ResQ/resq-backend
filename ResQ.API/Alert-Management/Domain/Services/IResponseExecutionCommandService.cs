using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IResponseExecutionCommandService
{
    Task<ResponseExecution?> Handle(DecideResponseAuthorizationCommand command);
}
