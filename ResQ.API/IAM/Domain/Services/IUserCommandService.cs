namespace ResQ.API.IAM.Domain.Services;

using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.Commands;

/// <summary>
///     Contract for commands that change user state (sign-in / sign-up).
/// </summary>
/// <remarks>
///     Implementations execute command objects and return domain results.
/// </remarks>
public interface IUserCommandService
{
    /// <summary>
    ///     Handle a sign-in operation.
    /// </summary>
    /// <param name="command">The sign-in command containing credentials.</param>
    /// <returns>A tuple containing the authenticated <see cref="User" /> and the JWT token string.</returns>
    Task<(User user, string token)> Handle(SignInCommand command);

    /// <summary>
    ///     Handle a sign-up operation to create a new user.
    /// </summary>
    /// <param name="command">The sign-up command with new user data.</param>
    /// <returns>A <see cref="Task" /> that completes when the operation finishes.</returns>
    Task Handle(SignUpCommand command);
}
