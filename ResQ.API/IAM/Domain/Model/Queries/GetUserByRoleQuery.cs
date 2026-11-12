using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Domain.Model.Queries;

/// <summary>Lists the users of one organization that have the given role.</summary>
public record GetUserByRoleQuery(Guid OrganizationId, Roles Role);
