namespace ResQ.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
/// Numeric value and engineering unit of a measurement.
/// </summary>
public record MeasurementValue
{
    public decimal Value { get; init; }

    public string Unit { get; init; }

    private MeasurementValue()
    {
        Unit = string.Empty;
    }

    public MeasurementValue(decimal value, string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Measurement unit is required.", nameof(unit));

        var normalizedUnit = unit.Trim().ToUpperInvariant();
        if (normalizedUnit.Length > 30)
            throw new ArgumentException("Measurement unit cannot exceed 30 characters.", nameof(unit));

        Value = value;
        Unit = normalizedUnit;
    }
}