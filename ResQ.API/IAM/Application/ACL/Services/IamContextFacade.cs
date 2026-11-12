using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Domain.Model.Queries;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Interfaces.ACL;
using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Application.ACL.Services;

public class IamContextFacade(IUserCommandService userCommandService, IUserQueryService userQueryService)
    : IIamContextFacade
{
    public async Task<int> CreateUser(Guid organizationId, string firstName, string lastName, string email,
        string username, string password, Roles role)
    {
        var command = new CreateMemberCommand(organizationId, firstName, lastName, email, username, password, role);
        return await userCommandService.Handle(command);
    }

    public async Task<int> FetchUserIdByUsername(string username)
    {
        var result = await userQueryService.Handle(new GetUserByUsernameQuery(username));
        return result?.Id ?? 0;
    }

    public async Task<string> FetchUsernameByUserId(int userId)
    {
        var result = await userQueryService.Handle(new GetUserByIdQuery(userId));
        return result?.Username ?? string.Empty;
    }

    public async Task<Guid?> FetchOrganizationIdByUserId(int userId)
    {
        var result = await userQueryService.Handle(new GetUserByIdQuery(userId));
        return result?.OrganizationId;
    }
}
