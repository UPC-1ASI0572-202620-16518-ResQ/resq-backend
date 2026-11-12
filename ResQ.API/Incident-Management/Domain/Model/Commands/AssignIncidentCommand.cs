namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record AssignIncidentCommand(Guid OrganizationId, Guid IncidentId, int AttendantId);
