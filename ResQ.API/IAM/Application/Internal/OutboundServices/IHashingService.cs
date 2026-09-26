namespace ResQ.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Contract for hashing and verifying passwords.
/// </summary>
public interface IHashingService
{
    /// <summary>
    ///     Creates a secure hash for the provided password.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The hashed password string.</returns>
    string HashPassword(string password);

    /// <summary>
    ///     Verifies that a plain-text password matches the provided hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="passwordHash">The stored password hash to compare against.</param>
    /// <returns><c>true</c> when the password matches the hash; otherwise <c>false</c>.</returns>
    bool VerifyPassword(string password, string passwordHash);
}
