using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;

namespace ResQ.API.Monitoring.Domain.Services;

public interface IMonitoringStateService
{
    void RecordMeasurement(DeviceMonitoringState state, Guid? zoneId, DateTimeOffset measuredAt, string? connectivityStatus);

    void RecordObservation(ZoneMonitoringState state, DateTimeOffset measuredAt);

    void MarkRisk(ZoneMonitoringState state, Guid riskDetectionId, string conditionCode);
}