using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Domain.Model.Commands;

namespace ResQ.API.Connectivity_Management.Domain.Services;

public interface IConnectivityCommandService
{
    /// <exception cref="KeyNotFoundException">When the sensor is not a registered device.</exception>
    Task<SensorConnection> Handle(RegisterHeartbeatCommand command);

    /// <returns>The connection, or null when the sensor never sent a heartbeat.</returns>
    Task<SensorConnection?> Handle(MarkSensorOfflineCommand command);

    /// <returns>The connections that changed to TIMEOUT.</returns>
    Task<IReadOnlyCollection<SensorConnection>> Handle(EvaluateSensorTimeoutsCommand command);
}
