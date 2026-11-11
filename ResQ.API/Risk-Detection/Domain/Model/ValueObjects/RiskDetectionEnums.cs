namespace ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

/// <summary>
/// Whether a detection rule participates in the evaluation of measurements (ACTIVE / INACTIVE).
/// </summary>
public enum EDetectionRuleStatus
{
    Active,
    Inactive
}

/// <summary>
/// Comparison performed by a detection condition
/// (GREATER_THAN, GREATER_THAN_OR_EQUAL, LESS_THAN, LESS_THAN_OR_EQUAL, EQUAL).
/// </summary>
public enum EComparisonOperator
{
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Equal
}

/// <summary>
/// Whether the location of a detection could be resolved from the source device (RESOLVED / UNRESOLVED).
/// </summary>
public enum ELocationResolutionStatus
{
    Resolved,
    Unresolved
}
