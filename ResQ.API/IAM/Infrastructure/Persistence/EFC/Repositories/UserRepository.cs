using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.IAM.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ResQ.API.IAM.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     Entity Framework Core repository for <see cref="User" /> aggregates.
/// </summary>
/// <remarks>
///     Provides common user-specific queries on top of <see cref="BaseRepository{T}" />.
/// </remarks>
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    /// <summary>
    ///     Finds a user by username asynchronously.
    /// </summary>
    /// <param name="username">The username to search.</param>
    /// <returns>The matching <see cref="User" /> or <c>null</c> if none found.</returns>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await Context.Set<User>().FirstOrDefaultAsync(user => user.Username.Equals(username));
    }

    /// <summary>
    ///     Checks whether a user exists with the given username.
    /// </summary>
    /// <param name="username">The username to search.</param>
    /// <returns><c>true</c> if a user with the username exists; otherwise <c>false</c>.</returns>
    public bool ExistsByUsername(string username)
    {
        return Context.Set<User>().Any(user => user.Username.Equals(username));
    }

    /// <summary>
    ///     Finds users by role asynchronously.
    /// </summary>
    /// <param name="role">The role to filter by.</param>
    /// <returns>A collection of users with the given role.</returns>
    public async Task<IEnumerable<User>> FindByRoleAsync(Roles role)
    {
        return await Context.Set<User>().Where(user => user.Role == role).ToListAsync();
    }
}
