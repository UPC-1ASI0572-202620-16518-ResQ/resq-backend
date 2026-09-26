using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.IAM.Domain.Repositories;

/// <summary>
///     Repository interface for <see cref="User" /> aggregates.
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    ///     Find a user by username asynchronously.
    /// </summary>
    /// <param name="username">The username to search.</param>
    /// <returns>The <see cref="User" /> when found; otherwise <c>null</c>.</returns>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>
    ///     Checks if a user exists by username.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns><c>true</c> if a user with the given username exists; otherwise <c>false</c>.</returns>
    bool ExistsByUsername(string username);

    /// <summary>
    ///     Find users by role asynchronously.
    /// </summary>
    /// <param name="role">The role to filter by.</param>
    /// <returns>A collection of users with the given role.</returns>
    Task<IEnumerable<User>> FindByRoleAsync(Roles role);
}
