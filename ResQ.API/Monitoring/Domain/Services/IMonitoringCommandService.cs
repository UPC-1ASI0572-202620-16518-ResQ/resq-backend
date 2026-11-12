using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.Commands;

namespace ResQ.API.Monitoring.Domain.Services;

public interface IMonitoringCommandService
{
    /// <exception cref="KeyNotFoundException">When the device does not exist in the organization.</exception>
    Task<(Measurement Measurement, IReadOnlyCollection<Guid> RiskDetectionIds)> Handle(RecordMeasurementCommand command);
}