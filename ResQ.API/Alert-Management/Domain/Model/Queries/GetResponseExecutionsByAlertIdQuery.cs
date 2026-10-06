namespace ResQ.API.Alert_Management.Domain.Model.Queries;

public record GetResponseExecutionsByAlertIdQuery(Guid OrganizationId, Guid AlertId);
