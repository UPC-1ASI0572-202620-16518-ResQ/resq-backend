namespace ResQ.API.IAM.Domain.Services;

using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.Commands;

public interface IUserCommandService
{
    Task<(User user, string token)> Handle(SignInCommand command);

    /// <summary>Creates a new organization and its Admin user. Returns the new user id.</summary>
    Task<int> Handle(SignUpCommand command);

    /// <summary>Creates a Citizen or Volunteer inside an existing organization. Returns the new user id.</summary>
    Task<int> Handle(CreateMemberCommand command);
}
