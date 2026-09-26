namespace ResQ.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query object used to request a user by username.
/// </summary>
/// <param name="Username">The username of the user to retrieve.</param>
public record GetUserByUsernameQuery(string Username);
