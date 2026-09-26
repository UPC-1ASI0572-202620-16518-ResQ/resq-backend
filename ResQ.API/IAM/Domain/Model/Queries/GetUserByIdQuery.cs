namespace ResQ.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query object used to request a user by identifier.
/// </summary>
/// <param name="Id">The identifier of the user to retrieve.</param>
public record GetUserByIdQuery(int Id);
