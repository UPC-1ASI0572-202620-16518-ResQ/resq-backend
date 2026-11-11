namespace ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

/// <summary>
/// Comparison performed over a monitored variable. It knows nothing about the physical sensor model.
/// </summary>
public record DetectionCondition
{
    public string VariableType { get; init; } = string.Empty;

    public EComparisonOperator Operator { get; init; }

    public decimal Threshold { get; init; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    private DetectionCondition()
    {
    }

    public static DetectionCondition Create(string? variableType, EComparisonOperator comparisonOperator, decimal threshold)
    {
        if (!Enum.IsDefined(comparisonOperator))
            throw new ArgumentException("Invalid comparison operator.");

        return new DetectionCondition
        {
            VariableType = RiskCodes.NormalizeVariableType(variableType),
            Operator = comparisonOperator,
            Threshold = threshold
        };
    }

    /// <summary>
    /// A condition is valid when it names a variable and a defined operator.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(VariableType) && Enum.IsDefined(Operator);
    }

    public bool Supports(string variableType)
    {
        return string.Equals(VariableType, variableType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True when the monitored value of the same variable satisfies the comparison against the threshold.
    /// </summary>
    public bool Matches(string variableType, decimal value)
    {
        if (!Supports(variableType)) return false;

        return Operator switch
        {
            EComparisonOperator.GreaterThan => value > Threshold,
            EComparisonOperator.GreaterThanOrEqual => value >= Threshold,
            EComparisonOperator.LessThan => value < Threshold,
            EComparisonOperator.LessThanOrEqual => value <= Threshold,
            EComparisonOperator.Equal => value == Threshold,
            _ => false
        };
    }
}
