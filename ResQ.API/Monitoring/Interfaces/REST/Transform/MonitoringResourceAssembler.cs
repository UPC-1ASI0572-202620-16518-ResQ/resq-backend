using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;
using ResQ.API.Monitoring.Domain.Services;
using ResQ.API.Monitoring.Interfaces.REST.Resources;

namespace ResQ.API.Monitoring.Interfaces.REST.Transform;

public static class MonitoringResourceAssembler
{
    public static MonitoringMeasurementResource ToResourceFromEntity(Measurement entity)
    {
        return new MonitoringMeasurementResource(
            entity.Id,
            entity.DeviceId,
            entity.BuildingId,
            entity.ZoneId,
            entity.VariableType,
            new MeasurementValueResource(
                entity.MeasurementValue.Value,
                entity.MeasurementValue.Unit),
            entity.MeasuredAt,
            entity.RecordedAt);
    }

    public static DeviceMonitoringStateResource ToResourceFromEntity(
        DeviceMonitoringState entity,
        EDeviceAvailability? availabilityOverride = null)
    {
        return new DeviceMonitoringStateResource(
            entity.DeviceId,
            entity.ZoneId,
            MonitoringCodes.ToCode(availabilityOverride ?? entity.Availability),
            entity.LastMeasurementAt,
            entity.UpdatedAt);
    }

    public static ZoneMonitoringStateResource ToResourceFromEntity(ZoneMonitoringState entity)
    {
        return new ZoneMonitoringStateResource(
            entity.ZoneId,
            entity.BuildingId,
            entity.LastUpdatedAt,
            entity.ActiveRiskId,
            entity.ConditionCode);
    }

    public static BuildingMonitoringStatusResource ToResourceFromSnapshot(BuildingMonitoringSnapshot snapshot)
    {
        return new BuildingMonitoringStatusResource(
            snapshot.BuildingId,
            snapshot.LastUpdatedAt,
            snapshot.Zones.Select(zone => ToResourceFromEntity(zone)).ToList(),
            snapshot.AvailableDevices,
            snapshot.UnavailableDevices,
            snapshot.UnknownDevices);
    }
}