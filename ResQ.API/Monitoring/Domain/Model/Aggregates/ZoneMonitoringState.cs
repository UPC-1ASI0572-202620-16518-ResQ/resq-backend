namespace ResQ.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
/// Current Monitoring observation state of a zone.
/// </summary>
public class ZoneMonitoringState
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid ZoneId { get; private set; }

    public Guid BuildingId { get; private set; }

    public DateTimeOffset LastUpdatedAt { get; private set; }

    public Guid? ActiveRiskId { get; private set; }

    public string ConditionCode { get; private set; } = "NORMAL";

    protected ZoneMonitoringState()
    {
    }

    private ZoneMonitoringState(Guid organizationId, Guid zoneId, Guid buildingId)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ZoneId = zoneId;
        BuildingId = buildingId;
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public static ZoneMonitoringState Create(Guid organizationId, Guid zoneId, Guid buildingId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.", nameof(organizationId));

        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone id is required.", nameof(zoneId));

        if (buildingId == Guid.Empty)
            throw new ArgumentException("Building id is required.", nameof(buildingId));

        return new ZoneMonitoringState(organizationId, zoneId, buildingId);
    }

    public void UpdateObservation(DateTimeOffset at)
    {
        if (at > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ArgumentException("Observation time cannot be more than 5 minutes in the future.", nameof(at));

        if (at > LastUpdatedAt)
            LastUpdatedAt = at;
    }

    public void MarkRiskActive(Guid riskDetectionId, string conditionCode)
    {
        if (riskDetectionId == Guid.Empty)
            throw new ArgumentException("Risk detection id is required.", nameof(riskDetectionId));

        if (string.IsNullOrWhiteSpace(conditionCode))
            throw new ArgumentException("Condition code is required.", nameof(conditionCode));

        ActiveRiskId = riskDetectionId;
        ConditionCode = conditionCode.Trim().ToUpperInvariant();
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearRisk()
    {
        ActiveRiskId = null;
        ConditionCode = "NORMAL";
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }
}