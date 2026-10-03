namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Snapshot of the risk detection that originated an alert.
/// RiskDetectionId references the Risk Detection context, which does not exist yet in this API,
/// so it is kept as an opaque identifier.
/// </summary>
public record AlertContext
{
    public string RiskDetectionId { get; init; } = string.Empty;

    public string RiskTypeCode { get; init; } = string.Empty;

    public string SeverityCode { get; init; } = string.Empty;

    public Guid? BuildingId { get; init; }

    public Guid? ZoneId { get; init; }

    public DateTimeOffset DetectedAt { get; init; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    private AlertContext()
    {
    }

    public static AlertContext Create(string riskDetectionId, string riskTypeCode, string severityCode, Guid? buildingId,
        Guid? zoneId, DateTimeOffset detectedAt)
    {
        if (string.IsNullOrWhiteSpace(riskDetectionId))
            throw new ArgumentException("Risk detection id is required.");

        if (riskDetectionId.Trim().Length > 64)
            throw new ArgumentException("Risk detection id cannot exceed 64 characters.");

        if (buildingId == Guid.Empty) buildingId = null;
        if (zoneId == Guid.Empty) zoneId = null;

        if (zoneId.HasValue && !buildingId.HasValue)
            throw new ArgumentException("A zone can only be referenced together with its building.");

        if (detectedAt == default)
            throw new ArgumentException("Detection date is required.");

        if (detectedAt > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ArgumentException("Detection date cannot be in the future.");

        return new AlertContext
        {
            RiskDetectionId = riskDetectionId.Trim(),
            RiskTypeCode = RiskCodes.NormalizeRiskTypeCode(riskTypeCode),
            SeverityCode = RiskCodes.NormalizeSeverityCode(severityCode),
            BuildingId = buildingId,
            ZoneId = zoneId,
            DetectedAt = detectedAt
        };
    }
}
