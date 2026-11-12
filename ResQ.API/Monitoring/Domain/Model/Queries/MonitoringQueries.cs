namespace ResQ.API.Monitoring.Domain.Model.Queries;

public record GetBuildingMonitoringStatusQuery(Guid OrganizationId, Guid BuildingId);

public record GetZoneMonitoringStatusQuery(Guid OrganizationId, Guid ZoneId);

public record GetDeviceMonitoringStatusQuery(Guid OrganizationId, Guid DeviceId);

public record GetCurrentDeviceMeasurementsQuery(Guid OrganizationId, Guid DeviceId, DateTimeOffset ReferenceTime);

public record GetCurrentZoneMeasurementsQuery(Guid OrganizationId, Guid ZoneId, DateTimeOffset ReferenceTime);

public record GetDeviceMeasurementHistoryQuery(
    Guid OrganizationId, Guid DeviceId, DateTimeOffset? From, DateTimeOffset? To, int Limit = 200);

public record GetZoneMeasurementHistoryQuery(
    Guid OrganizationId, Guid ZoneId, DateTimeOffset? From, DateTimeOffset? To, int Limit = 200);
