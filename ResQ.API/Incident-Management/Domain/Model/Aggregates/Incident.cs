using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Domain.Model.Aggregates;

public class Incident
{
    public IncidentId Id { get; private set; } = null!;

    public ZoneId ZoneId { get; private set; } = null!;

    public ERiskType Type { get; private set; }

    public ERiskLevel Level { get; private set; }

    public EIncidentStatus Status { get; private set; }

    public AttendantId? AssignedTo { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public string? ResolutionNotes { get; private set; }

    protected Incident()
    {
    }

    public static Incident Create(ZoneId zoneId, ERiskType type, ERiskLevel level)
    {
        ArgumentNullException.ThrowIfNull(zoneId);

        return new Incident
        {
            Id = IncidentId.New(),
            ZoneId = zoneId,
            Type = type,
            Level = level,
            Status = EIncidentStatus.Active,
            AssignedTo = null,
            CreatedAt = DateTimeOffset.UtcNow,
            ResolvedAt = null,
            ResolutionNotes = null
        };
    }

    public void AssignAttendant(AttendantId attendantId)
    {
        ArgumentNullException.ThrowIfNull(attendantId);

        if (Status != EIncidentStatus.Active)
            throw new InvalidOperationException("Only an active incident can be assigned.");

        AssignedTo = attendantId;
        Status = EIncidentStatus.InProgress;
    }

    public void ResolveIncident(string notes)
    {
        if (Status != EIncidentStatus.Active && Status != EIncidentStatus.InProgress)
        {
            throw new InvalidOperationException("The incident cannot be resolved in its current status.");
        }

        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Resolution notes are required.", nameof(notes));

        ResolutionNotes = notes.Trim();
        ResolvedAt = DateTimeOffset.UtcNow;
        Status = EIncidentStatus.Resolved;
    }
    
    public void ChangeStatus(EIncidentStatus newStatus)
    {
        if (Status == newStatus)
            return;

        // InProgress requires an attendant and Resolved requires notes, so they have their own operations
        if (newStatus == EIncidentStatus.InProgress)
            throw new InvalidOperationException("An incident moves to InProgress only when an attendant is assigned (use the assign operation).");

        if (newStatus == EIncidentStatus.Resolved)
            throw new InvalidOperationException("An incident is resolved only with resolution notes (use the resolve operation).");

        if (!IsValidStatusTransition(Status, newStatus))
        {
            throw new InvalidOperationException($"Invalid incident status transition: {Status} -> {newStatus}");
        }

        Status = newStatus;
        
        if (newStatus == EIncidentStatus.Resolved)
        {
            ResolvedAt = DateTimeOffset.UtcNow;
        }
    }

    public void ChangeRiskLevel(ERiskLevel newLevel)
    {
        EnsureNotClosed();

        if (Level == newLevel)
            return;

        Level = newLevel;
    }
    
    private static bool IsValidStatusTransition(EIncidentStatus current, EIncidentStatus next)
    {
        return
            current == EIncidentStatus.Active && next == EIncidentStatus.InProgress ||

            current == EIncidentStatus.Active && next == EIncidentStatus.Resolved ||

            current == EIncidentStatus.InProgress && next == EIncidentStatus.Resolved ||

            current == EIncidentStatus.Resolved && next == EIncidentStatus.Closed;
    }

    private void EnsureNotClosed()
    {
        if (Status == EIncidentStatus.Closed)
        {
            throw new InvalidOperationException("A closed incident cannot be modified.");
        }
    }
}