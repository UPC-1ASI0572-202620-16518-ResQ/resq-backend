using Microsoft.Extensions.Options;
using ResQ.API.Connectivity_Management.Interfaces.ACL;
using ResQ.API.Device_Management.Interfaces.ACL;
using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.Queries;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;
using ResQ.API.Monitoring.Domain.Repositories;
using ResQ.API.Monitoring.Domain.Services;

namespace ResQ.API.Monitoring.Application.Internal.QueryServices;

public class MonitoringQueryService(
    IMeasurementRepository measurementRepository,
    IDeviceMonitoringStateRepository deviceMonitoringStateRepository,
    IZoneMonitoringStateRepository zoneMonitoringStateRepository,
    IDevicesContextFacade devicesContextFacade,
    IConnectivityContextFacade connectivityContextFacade,
    IOptions<MonitoringSettings> settingsOptions) : IMonitoringQueryService
{
    public async Task<BuildingMonitoringSnapshot?> Handle(GetBuildingMonitoringStatusQuery query)
    {
        var zones = (await zoneMonitoringStateRepository.FindByBuildingIdAsync(query.OrganizationId, query.BuildingId)).ToList();
        var deviceStates = (await deviceMonitoringStateRepository.FindAllAsync(query.OrganizationId)).ToList();

        var buildingDeviceStates = new List<DeviceMonitoringState>();

        foreach (var state in deviceStates)
        {
            var device = await devicesContextFacade.GetDeviceCatalogEntry(query.OrganizationId, state.DeviceId);
            if (device is not null && device.BuildingId == query.BuildingId)
                buildingDeviceStates.Add(state);
        }

        if (zones.Count == 0 && buildingDeviceStates.Count == 0)
            return null;

        var availabilities = new List<EDeviceAvailability>();

        foreach (var state in buildingDeviceStates)
            availabilities.Add(await ResolveAvailabilityAsync(query.OrganizationId, state));

        var available = availabilities.Count(status => status == EDeviceAvailability.Available);
        var unavailable = availabilities.Count(status => status == EDeviceAvailability.Unavailable);
        var unknown = availabilities.Count(status => status == EDeviceAvailability.Unknown);

        var lastUpdatedAt = zones.Count > 0
            ? zones.Max(zone => zone.LastUpdatedAt)
            : buildingDeviceStates.Max(state => state.UpdatedAt);

        if (buildingDeviceStates.Count > 0)
            lastUpdatedAt = new[] { lastUpdatedAt, buildingDeviceStates.Max(state => state.UpdatedAt) }.Max();

        return new BuildingMonitoringSnapshot(
            query.BuildingId,
            lastUpdatedAt,
            zones,
            available,
            unavailable,
            unknown);
    }

    public async Task<ZoneMonitoringState?> Handle(GetZoneMonitoringStatusQuery query)
    {
        return await zoneMonitoringStateRepository.FindByZoneIdAsync(query.OrganizationId, query.ZoneId);
    }

    public async Task<DeviceMonitoringState?> Handle(GetDeviceMonitoringStatusQuery query)
    {
        var state = await deviceMonitoringStateRepository.FindByDeviceIdAsync(query.OrganizationId, query.DeviceId);
        if (state is null) return null;

        return state;
    }

    public async Task<IEnumerable<Measurement>> Handle(GetCurrentDeviceMeasurementsQuery query)
    {
        var minimumMeasuredAt = query.ReferenceTime - settingsOptions.Value.FreshnessPolicy.MaximumAge;
        var measurements = await measurementRepository.FindCurrentByDeviceIdAsync(
            query.OrganizationId,
            query.DeviceId,
            minimumMeasuredAt);

        return measurements
            .Where(measurement => measurement.IsCurrent(query.ReferenceTime, settingsOptions.Value.FreshnessPolicy))
            .GroupBy(measurement => measurement.VariableType, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(measurement => measurement.MeasuredAt)
                .ThenByDescending(measurement => measurement.RecordedAt)
                .First())
            .OrderBy(measurement => measurement.VariableType)
            .ToList();
    }

    public async Task<EDeviceAvailability?> GetEffectiveDeviceAvailabilityAsync(Guid organizationId, Guid deviceId)
    {
        var state = await deviceMonitoringStateRepository.FindByDeviceIdAsync(organizationId, deviceId);
        if (state is null) return null;

        return await ResolveAvailabilityAsync(organizationId, state);
    }

    private async Task<EDeviceAvailability> ResolveAvailabilityAsync(Guid organizationId, DeviceMonitoringState state)
    {
        var connectivityStatus = await connectivityContextFacade.GetSensorConnectionStatusAsync(organizationId, state.DeviceId);

        return connectivityStatus?.Trim().ToUpperInvariant() switch
        {
            "ONLINE" => EDeviceAvailability.Available,
            "OFFLINE" or "TIMEOUT" => EDeviceAvailability.Unavailable,
            _ => state.Availability
        };
    }

    public async Task<IEnumerable<Measurement>> Handle(GetCurrentZoneMeasurementsQuery query)
    {
        var minimumMeasuredAt = query.ReferenceTime - settingsOptions.Value.FreshnessPolicy.MaximumAge;
        var measurements = await measurementRepository.FindCurrentByZoneIdAsync(
            query.OrganizationId,
            query.ZoneId,
            minimumMeasuredAt);

        return measurements
            .Where(measurement => measurement.IsCurrent(query.ReferenceTime, settingsOptions.Value.FreshnessPolicy))
            .GroupBy(measurement => new { measurement.DeviceId, measurement.VariableType })
            .Select(group => group.OrderByDescending(measurement => measurement.MeasuredAt)
                .ThenByDescending(measurement => measurement.RecordedAt)
                .First())
            .OrderBy(measurement => measurement.DeviceId)
            .ThenBy(measurement => measurement.VariableType)
            .ToList();
    }
}