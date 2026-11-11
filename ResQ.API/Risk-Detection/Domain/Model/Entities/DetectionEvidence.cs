using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Domain.Model.Entities;

/// <summary>
/// Immutable snapshot of the monitored information that justified a detection.
/// Conceptually a value object; the Id only exists for persistence. MeasurementId and DeviceId are external references.
/// </summary>
public class DetectionEvidence
{
    public Guid Id { get; private set; }

    public Guid MeasurementId { get; private set; }

    public Guid DeviceId { get; private set; }

    public string VariableType { get; private set; } = string.Empty;

    public decimal Value { get; private set; }

    public DateTimeOffset MeasuredAt { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected DetectionEvidence()
    {
    }

    public static DetectionEvidence Create(Guid measurementId, Guid deviceId, string variableType, decimal value, DateTimeOffset measuredAt)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentException("Measurement id is required.");

        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device id is required.");

        if (measuredAt == default)
            throw new ArgumentException("Measurement date is required.");

        if (measuredAt > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ArgumentException("Measurement date cannot be in the future.");

        return new DetectionEvidence
        {
            Id = Guid.NewGuid(),
            MeasurementId = measurementId,
            DeviceId = deviceId,
            VariableType = RiskCodes.NormalizeVariableType(variableType),
            Value = value,
            MeasuredAt = measuredAt
        };
    }

    public bool MatchesVariable(string variableType)
    {
        return string.Equals(VariableType, variableType, StringComparison.OrdinalIgnoreCase);
    }
}
