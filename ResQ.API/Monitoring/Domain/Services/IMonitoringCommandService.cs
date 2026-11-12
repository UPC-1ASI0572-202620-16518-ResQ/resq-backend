using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Interfaces.ACL;

namespace ResQ.API.Monitoring.Domain.Services;

public interface IMonitoringCommandService
{
    /// <exception cref="KeyNotFoundException">When the device does not exist in the organization.</exception>
    Task<(Measurement Measurement, RiskEvaluationOutcome Evaluation)> Handle(RecordMeasurementCommand command);
}
