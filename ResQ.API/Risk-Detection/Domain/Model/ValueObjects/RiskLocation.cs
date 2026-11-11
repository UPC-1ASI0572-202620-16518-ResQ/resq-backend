namespace ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

/// <summary>
/// Location of a detection. Building and Zone are external references (Building Management owns them).
/// When the location cannot be resolved from the source device it is UNRESOLVED instead of invented.
/// </summary>
public record RiskLocation
{
    public Guid? BuildingId { get; init; }

    public Guid? ZoneId { get; init; }

    public ELocationResolutionStatus ResolutionStatus { get; init; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    private RiskLocation()
    {
    }

    public static RiskLocation Resolved(Guid buildingId, Guid? zoneId)
    {
        if (buildingId == Guid.Empty)
            throw new ArgumentException("A resolved location requires a building.");

        return new RiskLocation
        {
            BuildingId = buildingId,
            ZoneId = zoneId == Guid.Empty ? null : zoneId,
            ResolutionStatus = ELocationResolutionStatus.Resolved
        };
    }

    public static RiskLocation Unresolved()
    {
        return new RiskLocation { ResolutionStatus = ELocationResolutionStatus.Unresolved };
    }

    public bool IsResolved() => ResolutionStatus == ELocationResolutionStatus.Resolved;
}
