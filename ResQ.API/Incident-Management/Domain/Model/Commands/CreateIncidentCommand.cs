namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record CreateIncidentCommand(Guid OrganizationId, Guid ZoneId, string Type, string Level);