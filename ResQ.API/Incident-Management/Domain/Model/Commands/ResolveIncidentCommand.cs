namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record ResolveIncidentCommand(Guid OrganizationId, Guid IncidentId, string ResolutionNotes);
