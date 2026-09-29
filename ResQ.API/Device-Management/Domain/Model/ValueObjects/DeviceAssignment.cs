namespace ResQ.API.Device_Management.Domain.Model.ValueObjects;

public record DeviceAssignment
{
    public Guid BuildingId { get; }
    public Guid? ZoneId { get; }

    private DeviceAssignment()
    {
    }

    public DeviceAssignment(Guid buildingId, Guid? zoneId)
    {
        if (buildingId == Guid.Empty)
            throw new ArgumentException("Building id is required.", nameof(buildingId));

        if (zoneId.HasValue && zoneId.Value == Guid.Empty)
            throw new ArgumentException("Zone id cannot be empty.", nameof(zoneId));

        BuildingId = buildingId;
        ZoneId = zoneId;
    }
}