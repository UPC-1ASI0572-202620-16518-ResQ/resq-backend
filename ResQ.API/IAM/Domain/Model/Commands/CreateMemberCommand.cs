using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Domain.Model.Commands;

/// <summary>
///     An organization Admin adds a Citizen or Volunteer to their own organization.
/// </summary>
public record CreateMemberCommand(
    Guid OrganizationId,
    string FirstName,
    string LastName,
    string Email,
    string Username,
    string Password,
    Roles Role);
