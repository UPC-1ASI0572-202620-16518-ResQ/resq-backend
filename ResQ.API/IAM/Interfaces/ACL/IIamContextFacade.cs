using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Interfaces.ACL;

/// <summary>
///     Interface for the IAM context facade.
/// </summary>
public interface IIamContextFacade
{
    /// <summary>
    ///     Creates a new user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="role">The user role.</param>
    /// <returns>The user ID.</returns>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="email">The user's email.</param>
    Task<int> CreateUser(string firstName, string lastName, string email, string username, string password, Roles role);

    /// <summary>
    ///     Fetches the user ID by username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>The user ID.</returns>
    Task<int> FetchUserIdByUsername(string username);

    /// <summary>
    ///     Fetches the username by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The username.</returns>
    Task<string> FetchUsernameByUserId(int userId);
}
