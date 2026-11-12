namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource representing a user for REST API responses.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Username">The username of the user.</param>
/// <param name="Role">The role of the user (citizen, volunteer or admin).</param>
/// <param name="OrganizationId">The organization the user belongs to.</param>
public record UserResource(int Id, string Username, string Role, Guid OrganizationId);
