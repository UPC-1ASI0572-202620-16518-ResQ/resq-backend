using ResQ.API.IAM.Domain.Model.Aggregates;

namespace ResQ.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Contract to generate and validate JWT tokens for users.
/// </summary>
public interface ITokenService
{
    /// <summary>
    ///     Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for which to create the token.</param>
    /// <returns>The generated JWT token string.</returns>
    string GenerateToken(User user);

    /// <summary>
    ///     Validates the provided JWT token and returns the user id when valid.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>The user id if the token is valid; otherwise <c>null</c>.</returns>
    Task<int?> ValidateToken(string token);
}
