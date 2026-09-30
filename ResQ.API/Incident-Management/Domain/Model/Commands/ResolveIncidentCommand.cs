namespace ResQ.API.Incident_Management.Domain.Model.Commands;

public record ResolveIncidentCommand(Guid IncidentId, string ResolutionNotes);