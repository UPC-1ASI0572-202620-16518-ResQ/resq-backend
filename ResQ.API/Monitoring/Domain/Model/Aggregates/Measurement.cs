using ResQ.API.Monitoring.Domain.Model.ValueObjects;

namespace ResQ.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
/// Immutable monitored measurement captured from a device.
/// The original measuredAt and location snapshot are preserved even if the device is later reassigned.
/// </summary>
public class Measurement
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid DeviceId { get; private set; }

    public Guid BuildingId { get; private set; }

    public Guid? ZoneId { get; private set; }

    public string VariableType { get; private set; } = string.Empty;

    public MeasurementValue MeasurementValue { get; private set; } = null!;

    public DateTimeOffset MeasuredAt { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

    protected Measurement()
    {
    }

    private Measurement(
        Guid id,
        Guid organizationId,
        Guid deviceId,
        Guid buildingId,
        Guid? zoneId,
        string variableType,
        MeasurementValue measurementValue,
        DateTimeOffset measuredAt,
        DateTimeOffset recordedAt)
    {
        Id = id;
        OrganizationId = organizationId;
        DeviceId = deviceId;
        BuildingId = buildingId;
        ZoneId = zoneId;
        VariableType = variableType;
        MeasurementValue = measurementValue;
        MeasuredAt = measuredAt;
        RecordedAt = recordedAt;
    }

    public static Measurement Create(
        Guid organizationId,
        Guid deviceId,
        Guid buildingId,
        Guid? zoneId,
        string variableType,
        decimal value,
        string unit,
        DateTimeOffset measuredAt)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.", nameof(organizationId));

        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device id is required.", nameof(deviceId));

        if (buildingId == Guid.Empty)
            throw new ArgumentException("Building id is required.", nameof(buildingId));

        if (zoneId.HasValue && zoneId.Value == Guid.Empty)
            throw new ArgumentException("Zone id cannot be empty.", nameof(zoneId));

        var normalizedVariableType = MonitoringCodes.NormalizeVariableType(variableType);

        if (measuredAt > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ArgumentException("Measured at cannot be more than 5 minutes in the future.", nameof(measuredAt));

        return new Measurement(
            Guid.NewGuid(),
            organizationId,
            deviceId,
            buildingId,
            zoneId,
            normalizedVariableType,
            new MeasurementValue(value, unit),
            measuredAt,
            DateTimeOffset.UtcNow);
    }

    public bool IsCurrent(DateTimeOffset referenceTime, FreshnessPolicy freshnessPolicy)
    {
        ArgumentNullException.ThrowIfNull(freshnessPolicy);
        return freshnessPolicy.IsFresh(MeasuredAt, referenceTime);
    }

    public bool BelongsToDevice(Guid deviceId)
    {
        return DeviceId == deviceId;
    }

    public bool BelongsToZone(Guid zoneId)
    {
        return ZoneId.HasValue && ZoneId.Value == zoneId;
    }
}