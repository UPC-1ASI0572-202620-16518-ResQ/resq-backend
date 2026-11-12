using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Domain.Model.Aggregates;

public class IncidentSafetyVerification
{
    public Guid Id { get; private set; }
    public IncidentId IncidentId { get; private set; } = null!;
    public int VerifiedByUserId { get; private set; }
    public bool Safe { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid? DeviceId { get; private set; }
    public Guid? MeasurementId { get; private set; }
    public decimal? MeasuredValue { get; private set; }
    public DateTimeOffset? MeasuredAt { get; private set; }
    public string? RuleSnapshot { get; private set; }
    public DateTimeOffset VerifiedAt { get; private set; }

    protected IncidentSafetyVerification() { }

    public static IncidentSafetyVerification Create(
        Guid incidentId, int userId, bool safe, string reason, Guid? deviceId,
        Guid? measurementId, decimal? measuredValue, DateTimeOffset? measuredAt, string? ruleSnapshot)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Verification reason is required.");
        return new IncidentSafetyVerification
        {
            Id = Guid.NewGuid(), IncidentId = new IncidentId(incidentId), VerifiedByUserId = userId, Safe = safe,
            Reason = reason.Trim(), DeviceId = deviceId, MeasurementId = measurementId,
            MeasuredValue = measuredValue, MeasuredAt = measuredAt, RuleSnapshot = ruleSnapshot,
            VerifiedAt = DateTimeOffset.UtcNow
        };
    }
}
