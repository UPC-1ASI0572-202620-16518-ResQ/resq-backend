using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command used to create a new user (sign up).
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address.</param>
public record SignUpCommand(string FirstName, string LastName, string Email, string Username, string Password, Roles Role);
