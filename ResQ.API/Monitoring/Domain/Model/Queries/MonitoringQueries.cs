namespace ResQ.API.Monitoring.Domain.Model.Queries;

public record GetBuildingMonitoringStatusQuery(Guid OrganizationId, Guid BuildingId);

public record GetZoneMonitoringStatusQuery(Guid OrganizationId, Guid ZoneId);

public record GetDeviceMonitoringStatusQuery(Guid OrganizationId, Guid DeviceId);

public record GetCurrentDeviceMeasurementsQuery(Guid OrganizationId, Guid DeviceId, DateTimeOffset ReferenceTime);

public record GetCurrentZoneMeasurementsQuery(Guid OrganizationId, Guid ZoneId, DateTimeOffset ReferenceTime);