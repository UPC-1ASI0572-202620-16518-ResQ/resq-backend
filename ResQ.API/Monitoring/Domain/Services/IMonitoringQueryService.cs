using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;
using ResQ.API.Monitoring.Domain.Model.Queries;

namespace ResQ.API.Monitoring.Domain.Services;

public interface IMonitoringQueryService
{
    Task<BuildingMonitoringSnapshot?> Handle(GetBuildingMonitoringStatusQuery query);

    Task<ZoneMonitoringState?> Handle(GetZoneMonitoringStatusQuery query);

    Task<DeviceMonitoringState?> Handle(GetDeviceMonitoringStatusQuery query);

    Task<EDeviceAvailability?> GetEffectiveDeviceAvailabilityAsync(Guid organizationId, Guid deviceId);

    Task<IEnumerable<Measurement>> Handle(GetCurrentDeviceMeasurementsQuery query);

    Task<IEnumerable<Measurement>> Handle(GetCurrentZoneMeasurementsQuery query);

    Task<IEnumerable<Measurement>> Handle(GetDeviceMeasurementHistoryQuery query);

    Task<IEnumerable<Measurement>> Handle(GetZoneMeasurementHistoryQuery query);
}

/// <summary>
/// Query result for the building monitoring status endpoint.
/// </summary>
public record BuildingMonitoringSnapshot(
    Guid BuildingId,
    DateTimeOffset LastUpdatedAt,
    IReadOnlyCollection<ZoneMonitoringState> Zones,
    int AvailableDevices,
    int UnavailableDevices,
    int UnknownDevices);
