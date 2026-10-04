namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record ChangeResponsePolicyStatusCommand(Guid OrganizationId, Guid PolicyId, bool Active);
