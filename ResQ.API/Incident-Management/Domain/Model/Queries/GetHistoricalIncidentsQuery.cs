namespace ResQ.API.Incident_Management.Domain.Model.Queries;

public record GetHistoricalIncidentsQuery(Guid OrganizationId, Guid? ZoneId, DateOnly? StartDate, DateOnly? EndDate, string? Status);
