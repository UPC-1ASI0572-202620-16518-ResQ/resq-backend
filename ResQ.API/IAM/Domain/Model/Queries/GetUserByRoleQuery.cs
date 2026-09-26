using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query object used to request users by role.
/// </summary>
/// <param name="Role">The role to filter by.</param>
public record GetUserByRoleQuery(Roles Role);
