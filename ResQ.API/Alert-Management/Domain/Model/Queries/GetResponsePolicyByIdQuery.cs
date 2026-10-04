namespace ResQ.API.Alert_Management.Domain.Model.Queries;

public record GetResponsePolicyByIdQuery(Guid OrganizationId, Guid PolicyId);
