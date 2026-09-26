namespace ResQ.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command used to authenticate a user (sign in).
/// </summary>
/// <param name="Username">The username to authenticate.</param>
/// <param name="Password">The plain-text password to authenticate.</param>
public record SignInCommand(string Username, string Password);
