namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Resource representing a user for REST API responses.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Username">The username of the user.</param>
public record UserResource(int Id, string Username);
