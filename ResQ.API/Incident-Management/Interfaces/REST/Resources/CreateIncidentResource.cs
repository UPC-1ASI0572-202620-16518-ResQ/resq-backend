namespace ResQ.API.Incident_Management.Interfaces.REST.Resources;

public record CreateIncidentResource(Guid ZoneId, string Type, string Level);