namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource returned after a successful sign-in.
/// </summary>
public record AuthenticatedUserResource(int Id, string Username, string Token, string Role, Guid OrganizationId);
