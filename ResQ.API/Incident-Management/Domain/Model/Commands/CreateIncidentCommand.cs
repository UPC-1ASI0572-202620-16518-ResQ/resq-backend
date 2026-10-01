namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record CreateIncidentCommand(Guid ZoneId, string Type, string Level);