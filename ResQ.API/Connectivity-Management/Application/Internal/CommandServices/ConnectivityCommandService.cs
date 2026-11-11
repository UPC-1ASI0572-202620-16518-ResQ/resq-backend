using Microsoft.Extensions.Options;
using ResQ.API.Alert_Management.Interfaces.ACL;
using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Domain.Model.Commands;
using ResQ.API.Connectivity_Management.Domain.Model.Events;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Repositories;
using ResQ.API.Connectivity_Management.Domain.Services;
using ResQ.API.Device_Management.Interfaces.ACL;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Connectivity_Management.Application.Internal.CommandServices;

public class ConnectivityCommandService(
    ISensorConnectionRepository sensorConnectionRepository,
    IDevicesContextFacade devicesContextFacade,
    IAlertsContextFacade alertsContextFacade,
    IOptions<ConnectivitySettings> settings,
    IUnitOfWork unitOfWork,
    ILogger<ConnectivityCommandService> logger)
    : IConnectivityCommandService
{
    /// <summary>
    /// Risk type used for the alerts raised when a sensor loses its connection.
    /// </summary>
    public const string DisconnectionRiskTypeCode = "SENSOR_DISCONNECTED";

    public async Task<SensorConnection> Handle(RegisterHeartbeatCommand command)
    {
        // The sensor must be a device registered in Device Management (ACL)
        var device = await devicesContextFacade.GetDeviceCatalogEntry(command.OrganizationId, command.SensorId);

        if (device is null)
            throw new KeyNotFoundException($"Sensor '{command.SensorId}' is not a registered device.");

        var signalStrength = command.SignalStrength.HasValue
            ? SignalStrength.Create(command.SignalStrength.Value)
            : null;

        var connection = await sensorConnectionRepository.FindBySensorIdAsync(command.OrganizationId, command.SensorId);

        if (connection is null)
        {
            connection = SensorConnection.Open(command.OrganizationId, command.SensorId, command.Timestamp, signalStrength);
            await sensorConnectionRepository.AddAsync(connection);
        }
        else
        {
            connection.RegisterHeartbeat(command.Timestamp, signalStrength);
        }

        await unitOfWork.CompleteAsync();

        return connection;
    }

    public async Task<SensorConnection?> Handle(MarkSensorOfflineCommand command)
    {
        var connection = await sensorConnectionRepository.FindBySensorIdAsync(command.OrganizationId, command.SensorId);

        if (connection is null) return null;

        if (!connection.MarkAsOffline(DateTimeOffset.UtcNow)) return connection;

        await unitOfWork.CompleteAsync();
        await RaiseDisconnectionAlertsAsync([connection]);

        return connection;
    }

    public async Task<IReadOnlyCollection<SensorConnection>> Handle(EvaluateSensorTimeoutsCommand command)
    {
        var timeout = settings.Value.HeartbeatTimeout;
        var candidates = await sensorConnectionRepository.FindOnlineWithLastHeartbeatBeforeAsync(command.EvaluatedAt - timeout);

        var timedOut = candidates
            .Where(connection => connection.MarkAsTimedOut(command.EvaluatedAt, timeout))
            .ToList();

        if (timedOut.Count == 0) return timedOut;

        await unitOfWork.CompleteAsync();
        await RaiseDisconnectionAlertsAsync(timedOut);

        return timedOut;
    }

    /// <summary>
    /// Raises one disconnection alert per ConnectionLostEvent through the Alert Management ACL.
    /// A failure here never undoes the status change already persisted.
    /// </summary>
    private async Task RaiseDisconnectionAlertsAsync(IEnumerable<SensorConnection> connections)
    {
        if (!settings.Value.RaiseDisconnectionAlerts) return;

        foreach (var connection in connections)
        {
            foreach (var lost in connection.DomainEvents.OfType<ConnectionLostEvent>())
            {
                try
                {
                    var device = await devicesContextFacade.GetDeviceCatalogEntry(connection.OrganizationId, connection.SensorId);

                    var alertId = await alertsContextFacade.GenerateAlertAsync(
                        connection.OrganizationId,
                        $"CONNECTIVITY-{connection.SensorId:N}",
                        DisconnectionRiskTypeCode,
                        "Warning",
                        device?.BuildingId,
                        device?.ZoneId,
                        lost.OccurredAt,
                        []);

                    logger.LogInformation("Sensor {SensorId} is {Status}; disconnection alert {AlertId} generated.",
                        connection.SensorId, lost.NewStatus, alertId);
                }
                catch (Exception e) when (e is ArgumentException or InvalidOperationException)
                {
                    logger.LogWarning(e, "Could not generate the disconnection alert for sensor {SensorId}.", connection.SensorId);
                }
            }
        }
    }
}
