namespace ResQ.API.Alert_Management.Domain.Model.Queries;

public record GetAlertByIdQuery(Guid OrganizationId, Guid AlertId);
