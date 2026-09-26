using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Domain.Model.Queries;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Interfaces.ACL;
using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Application.ACL.Services;

/// <summary>
///     Facade for the IAM context, providing access to user-related operations.
/// </summary>
public class IamContextFacade(IUserCommandService userCommandService, IUserQueryService userQueryService)
    : IIamContextFacade
{
    /// <summary>
    ///     Creates a new user with the specified username, password, and role.
    /// </summary>
    /// <param name="username">The username for the new user.</param>
    /// <param name="password">The password for the new user.</param>
    /// <param name="role">The role for the new user.</param>
    /// <returns>The ID of the created user, or 0 if creation failed.</returns>
    public async Task<int> CreateUser(string username, string password, Roles role)
    {
        var signUpCommand = new SignUpCommand(username, password, role);
        await userCommandService.Handle(signUpCommand);
        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery);
        return result?.Id ?? 0;
    }

    /// <summary>
    ///     Fetches the user ID by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user ID if found, otherwise 0.</returns>
    public async Task<int> FetchUserIdByUsername(string username)
    {
        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery);
        return result?.Id ?? 0;
    }

    /// <summary>
    ///     Fetches the username by user ID.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <returns>The username if found, otherwise an empty string.</returns>
    public async Task<string> FetchUsernameByUserId(int userId)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var result = await userQueryService.Handle(getUserByIdQuery);
        return result?.Username ?? string.Empty;
    }
}
