namespace ResQ.API.Risk_Detection.Interfaces.REST.Resources;

/// <summary>
/// Detection rule returned by the API. Matches DetectionRuleResourceDto in the frontend.
/// Status is ACTIVE or INACTIVE.
/// </summary>
public record DetectionRuleResource(
    Guid RuleId,
    Guid? ZoneId,
    string RiskTypeCode,
    string SeverityCode,
    DetectionConditionResource Condition,
    string Status);

/// <summary>
/// Matches DetectionConditionResourceDto in the frontend.
/// Operator is GREATER_THAN, GREATER_THAN_OR_EQUAL, LESS_THAN, LESS_THAN_OR_EQUAL or EQUAL.
/// </summary>
public record DetectionConditionResource(
    string VariableType,
    string Operator,
    decimal Threshold);

/// <summary>
/// Payload to configure a rule. Matches ConfigureDetectionRuleResourceDto in the frontend.
/// </summary>
public record ConfigureDetectionRuleResource(
    string RiskTypeCode,
    string SeverityCode,
    string VariableType,
    string Operator,
    decimal Threshold);

/// <summary>
/// Payload to activate/deactivate a rule. Matches ChangeDetectionRuleStatusResourceDto in the frontend.
/// </summary>
public record ChangeDetectionRuleStatusResource(bool? Active);
