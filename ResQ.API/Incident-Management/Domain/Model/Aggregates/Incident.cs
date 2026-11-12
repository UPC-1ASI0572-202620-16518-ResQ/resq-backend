using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Domain.Model.Aggregates;

public class Incident
{
    public IncidentId Id { get; private set; } = null!;

    public ZoneId ZoneId { get; private set; } = null!;

    public Guid OrganizationId { get; private set; }

    public Guid? RiskDetectionId { get; private set; }

    public Guid? AlertId { get; private set; }

    public ERiskType Type { get; private set; }

    public ERiskLevel Level { get; private set; }

    public EIncidentStatus Status { get; private set; }

    public AttendantId? AssignedTo { get; private set; }

    public int? AssignedUserId { get; private set; }

    public DateTimeOffset? AssignedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public int? ResolvedByUserId { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public int? ClosedByUserId { get; private set; }

    public string? ResolutionNotes { get; private set; }

    private readonly List<IncidentHumanAction> _humanActions = [];
    public IReadOnlyCollection<IncidentHumanAction> HumanActions => _humanActions.AsReadOnly();

    private readonly List<IncidentSafetyVerification> _safetyVerifications = [];
    public IReadOnlyCollection<IncidentSafetyVerification> SafetyVerifications => _safetyVerifications.AsReadOnly();

    public bool IsSafetyVerified => _safetyVerifications
        .OrderByDescending(verification => verification.VerifiedAt)
        .FirstOrDefault()?.Safe == true;

    protected Incident()
    {
    }

    public static Incident Create(
        Guid organizationId,
        ZoneId zoneId,
        ERiskType type,
        ERiskLevel level,
        Guid? riskDetectionId = null,
        Guid? alertId = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.", nameof(organizationId));

        ArgumentNullException.ThrowIfNull(zoneId);

        return new Incident
        {
            Id = IncidentId.New(),
            OrganizationId = organizationId,
            ZoneId = zoneId,
            RiskDetectionId = riskDetectionId,
            AlertId = alertId,
            Type = type,
            Level = level,
            Status = EIncidentStatus.Active,
            AssignedTo = null,
            AssignedUserId = null,
            AssignedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            ResolvedAt = null,
            ResolvedByUserId = null,
            ClosedAt = null,
            ClosedByUserId = null,
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

    public void AssignCurrentUser(int userId)
    {
        if (userId <= 0) throw new ArgumentException("User id is required.", nameof(userId));
        if (Status != EIncidentStatus.Active && Status != EIncidentStatus.InProgress)
            throw new InvalidOperationException("Only an active incident can be assumed.");
        if (AssignedUserId.HasValue && AssignedUserId.Value != userId)
            throw new InvalidOperationException("The incident is already assigned to another user.");

        AssignedUserId = userId;
        AssignedAt ??= DateTimeOffset.UtcNow;
        Status = EIncidentStatus.InProgress;
    }

    public void ResolveIncident(string notes, int? resolvedByUserId = null)
    {
        if (Status != EIncidentStatus.Active && Status != EIncidentStatus.InProgress)
        {
            throw new InvalidOperationException("The incident cannot be resolved in its current status.");
        }

        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Resolution notes are required.", nameof(notes));

        if (Level == ERiskLevel.Critical && AssignedUserId is null)
            throw new InvalidOperationException("A critical incident must be assigned before it can be resolved.");

        if (Level == ERiskLevel.Critical && !IsSafetyVerified)
            throw new InvalidOperationException("A critical incident requires a successful server safety verification before resolution.");

        ResolutionNotes = notes.Trim();
        ResolvedAt = DateTimeOffset.UtcNow;
        ResolvedByUserId = resolvedByUserId;
        Status = EIncidentStatus.Resolved;
    }

    public IncidentHumanAction AddHumanAction(int userId, string actionCode, string? notes)
    {
        EnsureNotClosed();
        if (AssignedUserId != userId)
            throw new InvalidOperationException("Only the assigned user can record actions on this incident.");

        var action = IncidentHumanAction.Create(Id.Value, userId, actionCode, notes);
        _humanActions.Add(action);
        return action;
    }

    public IncidentSafetyVerification RecordSafetyVerification(
        int userId,
        bool safe,
        string reason,
        Guid? deviceId,
        Guid? measurementId,
        decimal? measuredValue,
        DateTimeOffset? measuredAt,
        string? ruleSnapshot)
    {
        EnsureNotClosed();
        if (AssignedUserId != userId)
            throw new InvalidOperationException("Only the assigned user can verify the incident.");

        var verification = IncidentSafetyVerification.Create(
            Id.Value, userId, safe, reason, deviceId, measurementId, measuredValue, measuredAt, ruleSnapshot);
        _safetyVerifications.Add(verification);
        return verification;
    }
    
    public void ChangeStatus(EIncidentStatus newStatus, int? changedByUserId = null)
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

        if (newStatus == EIncidentStatus.Closed)
        {
            ClosedAt = DateTimeOffset.UtcNow;
            ClosedByUserId = changedByUserId;
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
