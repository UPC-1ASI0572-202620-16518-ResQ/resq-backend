namespace ResQ.API.Alert_Management.Domain.Model.Queries;

public record GetResponsePoliciesQuery(Guid OrganizationId, string? RiskTypeCode);
