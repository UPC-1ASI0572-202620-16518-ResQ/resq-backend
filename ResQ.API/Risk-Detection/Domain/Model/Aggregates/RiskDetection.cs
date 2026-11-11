using ResQ.API.Risk_Detection.Domain.Model.Entities;
using ResQ.API.Risk_Detection.Domain.Model.Events;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that represents a detected risk with its current severity, location,
/// the evidence that originated it and the traceability of its severity changes.
/// A risk detection never becomes an Incident.
/// </summary>
public class RiskDetection
{
    private readonly List<DetectionEvidence> _evidence = new();
    private readonly List<SeverityChange> _severityChanges = new();
    private readonly List<IRiskDetectionDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Rule that produced the current severity.
    /// </summary>
    public Guid RuleId { get; private set; }

    public string RiskTypeCode { get; private set; } = string.Empty;

    public string CurrentSeverityCode { get; private set; } = string.Empty;

    public RiskLocation Location { get; private set; } = null!;

    /// <summary>
    /// Measurement time of the first evidence.
    /// </summary>
    public DateTimeOffset DetectedAt { get; private set; }

    /// <summary>
    /// Measurement time of the most recent evidence; used to correlate new measurements with this detection.
    /// </summary>
    public DateTimeOffset LastEvidenceAt { get; private set; }

    public IReadOnlyCollection<DetectionEvidence> Evidence => _evidence.AsReadOnly();

    public IReadOnlyCollection<SeverityChange> SeverityChanges => _severityChanges.AsReadOnly();

    /// <summary>
    /// Domain events raised since the aggregate was loaded. Not persisted.
    /// </summary>
    public IReadOnlyCollection<IRiskDetectionDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected RiskDetection()
    {
    }

    /// <summary>
    /// Creates a detection from a satisfied rule. The detection uses the rule's RiskType and SeverityLevel.
    /// </summary>
    public static RiskDetection Detect(DetectionRule rule, RiskLocation location, DetectionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(evidence);

        var detection = new RiskDetection
        {
            Id = Guid.NewGuid(),
            OrganizationId = rule.OrganizationId,
            RuleId = rule.Id,
            RiskTypeCode = rule.RiskTypeCode,
            CurrentSeverityCode = rule.SeverityCode,
            Location = location,
            DetectedAt = evidence.MeasuredAt,
            LastEvidenceAt = evidence.MeasuredAt
        };

        detection._evidence.Add(evidence);
        detection._domainEvents.Add(new RiskDetectedEvent(
            detection.Id, rule.Id, detection.RiskTypeCode, detection.CurrentSeverityCode,
            location.BuildingId, location.ZoneId, DateTimeOffset.UtcNow));

        return detection;
    }

    /// <summary>
    /// Adds monitored evidence that participates in the evolution of the same detection.
    /// </summary>
    public void AddEvidence(DetectionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        if (_evidence.Any(existing => existing.MeasurementId == evidence.MeasurementId))
            return;

        _evidence.Add(evidence);

        if (evidence.MeasuredAt > LastEvidenceAt) LastEvidenceAt = evidence.MeasuredAt;
    }

    /// <summary>
    /// Updates the current severity preserving the previous value as a SeverityChange.
    /// </summary>
    /// <returns>True when the severity changed.</returns>
    public bool UpdateSeverity(Guid ruleId, string newSeverityCode, DateTimeOffset changedAt)
    {
        var normalized = RiskCodes.NormalizeSeverityCode(newSeverityCode);

        if (HasSeverity(normalized)) return false;

        var previous = CurrentSeverityCode;

        _severityChanges.Add(new SeverityChange(previous, normalized, changedAt));
        CurrentSeverityCode = normalized;
        RuleId = ruleId;

        if (RiskCodes.SeverityRank(normalized) > RiskCodes.SeverityRank(previous))
        {
            _domainEvents.Add(new RiskSeverityEscalatedEvent(
                Id, ruleId, RiskTypeCode, previous, normalized, Location.BuildingId, Location.ZoneId, changedAt));
        }

        return true;
    }

    public bool HasSeverity(string severityCode)
    {
        return string.Equals(CurrentSeverityCode, severityCode, StringComparison.OrdinalIgnoreCase);
    }
}
