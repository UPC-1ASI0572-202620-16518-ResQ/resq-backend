namespace ResQ.API.IAM.Domain.Model.Queries;

/// <summary>Lists the users of one organization.</summary>
public record GetAllUsersQuery(Guid OrganizationId);
