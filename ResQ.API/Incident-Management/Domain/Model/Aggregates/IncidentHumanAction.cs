using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Domain.Model.Aggregates;

public class IncidentHumanAction
{
    public Guid Id { get; private set; }
    public IncidentId IncidentId { get; private set; } = null!;
    public int UserId { get; private set; }
    public string ActionCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTimeOffset PerformedAt { get; private set; }

    protected IncidentHumanAction() { }

    public static IncidentHumanAction Create(Guid incidentId, int userId, string actionCode, string? notes)
    {
        if (incidentId == Guid.Empty) throw new ArgumentException("Incident id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        if (string.IsNullOrWhiteSpace(actionCode)) throw new ArgumentException("Action code is required.");

        return new IncidentHumanAction
        {
            Id = Guid.NewGuid(), IncidentId = new IncidentId(incidentId), UserId = userId,
            ActionCode = actionCode.Trim().ToUpperInvariant(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            PerformedAt = DateTimeOffset.UtcNow
        };
    }
}
