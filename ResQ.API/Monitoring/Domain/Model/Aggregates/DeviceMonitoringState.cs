using ResQ.API.Monitoring.Domain.Model.ValueObjects;

namespace ResQ.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
/// Current Monitoring state of a device.
/// </summary>
public class DeviceMonitoringState
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid DeviceId { get; private set; }

    public Guid? ZoneId { get; private set; }

    public EDeviceAvailability Availability { get; private set; }

    public DateTimeOffset? LastMeasurementAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    protected DeviceMonitoringState()
    {
    }

    private DeviceMonitoringState(Guid organizationId, Guid deviceId, Guid? zoneId)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        DeviceId = deviceId;
        ZoneId = zoneId;
        Availability = EDeviceAvailability.Unknown;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static DeviceMonitoringState Create(Guid organizationId, Guid deviceId, Guid? zoneId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.", nameof(organizationId));

        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device id is required.", nameof(deviceId));

        if (zoneId.HasValue && zoneId.Value == Guid.Empty)
            throw new ArgumentException("Zone id cannot be empty.", nameof(zoneId));

        return new DeviceMonitoringState(organizationId, deviceId, zoneId);
    }

    public void RecordCommunication(DateTimeOffset measuredAt)
    {
        if (LastMeasurementAt is null || measuredAt > LastMeasurementAt.Value)
            LastMeasurementAt = measuredAt;

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateZone(Guid? zoneId)
    {
        if (zoneId.HasValue && zoneId.Value == Guid.Empty)
            throw new ArgumentException("Zone id cannot be empty.", nameof(zoneId));

        ZoneId = zoneId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkUnavailable()
    {
        Availability = EDeviceAvailability.Unavailable;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAvailable()
    {
        Availability = EDeviceAvailability.Available;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkUnknown()
    {
        Availability = EDeviceAvailability.Unknown;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsAvailable()
    {
        return Availability == EDeviceAvailability.Available;
    }
}