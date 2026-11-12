using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that represents a configurable rule: when a monitored variable satisfies its condition,
/// a risk of the rule's type and severity is detected.
/// </summary>
public class DetectionRule
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid? ZoneId { get; private set; }

    /// <summary>
    /// RiskType code (e.g. GAS_LEAK, FIRE).
    /// </summary>
    public string RiskTypeCode { get; private set; } = string.Empty;

    /// <summary>
    /// SeverityLevel code produced when the rule is satisfied (Info, Warning, Critical).
    /// </summary>
    public string SeverityCode { get; private set; } = string.Empty;

    public DetectionCondition Condition { get; private set; } = null!;

    public EDetectionRuleStatus Status { get; private set; }

    public DateTimeOffset ConfiguredAt { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected DetectionRule()
    {
    }

    /// <summary>
    /// Configures a new rule. A rule with a valid condition starts ACTIVE.
    /// </summary>
    public static DetectionRule Configure(
        Guid organizationId, string riskTypeCode, string severityCode, DetectionCondition condition, Guid? zoneId = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        ArgumentNullException.ThrowIfNull(condition);

        if (!condition.IsValid())
            throw new ArgumentException("The detection condition is not valid.");

        return new DetectionRule
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ZoneId = zoneId,
            RiskTypeCode = RiskCodes.NormalizeRiskTypeCode(riskTypeCode),
            SeverityCode = RiskCodes.NormalizeSeverityCode(severityCode),
            Condition = condition,
            Status = EDetectionRuleStatus.Active,
            ConfiguredAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateCondition(DetectionCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        if (!condition.IsValid())
            throw new ArgumentException("The detection condition is not valid.");

        Condition = condition;
    }

    /// <summary>
    /// Activates the rule only when its configuration is valid.
    /// </summary>
    public void Activate()
    {
        if (Condition is null || !Condition.IsValid())
            throw new InvalidOperationException("A detection rule with an invalid condition cannot be activated.");

        Status = EDetectionRuleStatus.Active;
    }

    public void Deactivate()
    {
        Status = EDetectionRuleStatus.Inactive;
    }

    public bool IsActive() => Status == EDetectionRuleStatus.Active;

    public bool Supports(string variableType) => Condition.Supports(variableType);
}
