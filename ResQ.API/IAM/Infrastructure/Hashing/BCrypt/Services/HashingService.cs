using ResQ.API.IAM.Application.Internal.OutboundServices;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ResQ.API.IAM.Infrastructure.Hashing.BCrypt.Services;

/// <summary>
///     Implements <see cref="IHashingService" /> using BCrypt.
/// </summary>
public class HashingService : IHashingService
{
    /// <summary>
    ///     Hashes a plain-text password using BCrypt.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The resulting password hash.</returns>
    public string HashPassword(string password)
    {
        return BCryptNet.HashPassword(password);
    }

    /// <summary>
    ///     Verifies that the provided plain-text password matches the stored hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="passwordHash">The stored password hash to compare against.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCryptNet.Verify(password, passwordHash);
    }
}
