namespace ResQ.API.IAM.Domain.Model.Commands;

/// <summary>
///     Public sign-up: registers a new organization and its first user, who becomes its Admin.
/// </summary>
/// <param name="FirstName">First name for the profile.</param>
/// <param name="LastName">Last name for the profile.</param>
/// <param name="Email">Contact email for the profile.</param>
/// <param name="Username">Unique username.</param>
/// <param name="Password">Plain-text password (validated and hashed).</param>
/// <param name="OrganizationName">Name of the new organization; defaults to "{username} organization".</param>
public record SignUpCommand(
    string FirstName,
    string LastName,
    string Email,
    string Username,
    string Password,
    string? OrganizationName);
