using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IResponsePolicyCommandService
{
    Task<ResponsePolicy?> Handle(ConfigureResponsePolicyCommand command);

    Task<ResponsePolicy?> Handle(UpdateResponsePolicyCommand command);

    Task<ResponsePolicy?> Handle(ChangeResponsePolicyStatusCommand command);
}
