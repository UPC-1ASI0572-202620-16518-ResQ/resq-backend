using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;

namespace ResQ.API.Monitoring.Domain.Services;

public class MonitoringStateService : IMonitoringStateService
{
    public void RecordMeasurement(
        DeviceMonitoringState state,
        Guid? zoneId,
        DateTimeOffset measuredAt,
        string? connectivityStatus)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.UpdateZone(zoneId);
        state.RecordCommunication(measuredAt);

        switch (connectivityStatus?.Trim().ToUpperInvariant())
        {
            case "ONLINE":
                state.MarkAvailable();
                break;
            case "OFFLINE":
            case "TIMEOUT":
                state.MarkUnavailable();
                break;
            default:
                state.MarkUnknown();
                break;
        }
    }

    public void RecordObservation(ZoneMonitoringState state, DateTimeOffset measuredAt)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.UpdateObservation(measuredAt);
    }

    public void MarkRisk(ZoneMonitoringState state, Guid riskDetectionId, string conditionCode)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.MarkRiskActive(riskDetectionId, conditionCode);
    }
}