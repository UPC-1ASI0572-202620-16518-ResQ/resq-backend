namespace ResQ.API.Monitoring.Interfaces.REST.Resources;

public record RecordMeasurementResource(
    Guid DeviceId,
    string VariableType,
    decimal? Value,
    string Unit,
    DateTimeOffset MeasuredAt);

public record MonitoringMeasurementResource(
    Guid MeasurementId,
    Guid DeviceId,
    Guid BuildingId,
    Guid? ZoneId,
    string VariableType,
    MeasurementValueResource MeasurementValue,
    DateTimeOffset MeasuredAt,
    DateTimeOffset RecordedAt);

public record MeasurementValueResource(
    decimal Value,
    string Unit);

public record MeasurementIngestionResource(
    Guid MeasurementId,
    Guid DeviceId,
    Guid BuildingId,
    Guid? ZoneId,
    string VariableType,
    MeasurementValueResource MeasurementValue,
    DateTimeOffset MeasuredAt,
    DateTimeOffset RecordedAt,
    IEnumerable<Guid> RiskDetectionIds,
    IEnumerable<Guid> GeneratedAlertIds,
    IEnumerable<Guid> GeneratedIncidentIds);

public record DeviceMonitoringStateResource(
    Guid DeviceId,
    Guid? ZoneId,
    string Availability,
    DateTimeOffset? LastMeasurementAt,
    DateTimeOffset UpdatedAt);

public record ZoneMonitoringStateResource(
    Guid ZoneId,
    Guid BuildingId,
    DateTimeOffset LastUpdatedAt,
    Guid? ActiveRiskId,
    string ConditionCode);

public record BuildingMonitoringStatusResource(
    Guid BuildingId,
    DateTimeOffset LastUpdatedAt,
    IEnumerable<ZoneMonitoringStateResource> Zones,
    int AvailableDevices,
    int UnavailableDevices,
    int UnknownDevices);
