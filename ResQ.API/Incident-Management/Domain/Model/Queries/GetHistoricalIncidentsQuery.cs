namespace ResQ.API.Incident_Management.Domain.Model.Queries;

public record GetHistoricalIncidentsQuery(Guid? ZoneId, DateOnly? StartDate, DateOnly? EndDate, string? Status);