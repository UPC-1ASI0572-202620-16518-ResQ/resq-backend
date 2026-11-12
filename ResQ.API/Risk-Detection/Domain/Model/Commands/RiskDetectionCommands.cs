using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Domain.Model.Commands;

/// <summary>
/// Configures a new detection rule.
/// </summary>
public record ConfigureDetectionRuleCommand(
    Guid OrganizationId,
    string RiskTypeCode,
    string SeverityCode,
    string VariableType,
    EComparisonOperator Operator,
    decimal Threshold,
    Guid? ZoneId = null);

/// <summary>
/// Activates or deactivates an existing detection rule.
/// </summary>
public record ChangeDetectionRuleStatusCommand(Guid OrganizationId, Guid RuleId, bool Active);

/// <summary>
/// Evaluates a valid monitored measurement against the active rules of the organization.
/// </summary>
public record EvaluateMeasurementCommand(
    Guid OrganizationId,
    Guid MeasurementId,
    Guid DeviceId,
    string VariableType,
    decimal Value,
    DateTimeOffset MeasuredAt);
