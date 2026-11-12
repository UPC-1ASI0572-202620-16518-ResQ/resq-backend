using ResQ.API.Connectivity_Management.Interfaces.ACL;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Interfaces.ACL;
using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.Commands;
using ResQ.API.Monitoring.Domain.Repositories;
using ResQ.API.Monitoring.Domain.Services;
using ResQ.API.Monitoring.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Interfaces.ACL;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Monitoring.Application.Internal.CommandServices;

public class MonitoringCommandService(
    IMeasurementRepository measurementRepository,
    IDeviceMonitoringStateRepository deviceMonitoringStateRepository,
    IZoneMonitoringStateRepository zoneMonitoringStateRepository,
    IDevicesContextFacade devicesContextFacade,
    IConnectivityContextFacade connectivityContextFacade,
    IRiskDetectionContextFacade riskDetectionContextFacade,
    IMonitoringStateService monitoringStateService,
    IUnitOfWork unitOfWork) : IMonitoringCommandService
{
    public async Task<(Measurement Measurement, RiskEvaluationOutcome Evaluation)> Handle(RecordMeasurementCommand command)
    {
        var device = await devicesContextFacade.GetDeviceCatalogEntry(command.OrganizationId, command.DeviceId);

        if (device is null)
            throw new KeyNotFoundException($"Device '{command.DeviceId}' was not found.");

        if (device.AdministrativeStatus != EDeviceAdministrativeStatus.Active)
            throw new InvalidOperationException("The device must be active to receive measurements.");

        var variableType = MonitoringCodes.NormalizeVariableType(command.VariableType);
        var capability = device.Capabilities.FirstOrDefault(candidate =>
            candidate.Kind == ECapabilityKind.Measurement &&
            string.Equals(candidate.Code, variableType, StringComparison.OrdinalIgnoreCase));

        if (capability is null)
            throw new ArgumentException($"Device '{command.DeviceId}' does not have a measurement capability for '{variableType}'.");

        if (!string.IsNullOrWhiteSpace(capability.Unit) &&
            !string.Equals(capability.Unit, command.Unit, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Measurement unit '{command.Unit}' does not match the device capability unit '{capability.Unit}'.");
        }

        var measurement = Measurement.Create(
            command.OrganizationId,
            command.DeviceId,
            device.BuildingId,
            device.ZoneId,
            variableType,
            command.Value,
            command.Unit,
            command.MeasuredAt);

        await measurementRepository.AddAsync(measurement);
        await unitOfWork.CompleteAsync();

        var connectivityStatus = await connectivityContextFacade.GetSensorConnectionStatusAsync(
            command.OrganizationId,
            command.DeviceId);

        var deviceState = await deviceMonitoringStateRepository.FindByDeviceIdAsync(
            command.OrganizationId,
            command.DeviceId);

        if (deviceState is null)
        {
            deviceState = DeviceMonitoringState.Create(
                command.OrganizationId,
                command.DeviceId,
                device.ZoneId);

            await deviceMonitoringStateRepository.AddAsync(deviceState);
        }

        monitoringStateService.RecordMeasurement(
            deviceState,
            device.ZoneId,
            command.MeasuredAt,
            connectivityStatus);

        ZoneMonitoringState? zoneState = null;

        if (device.ZoneId.HasValue)
        {
            zoneState = await zoneMonitoringStateRepository.FindByZoneIdAsync(
                command.OrganizationId,
                device.ZoneId.Value);

            if (zoneState is null)
            {
                zoneState = ZoneMonitoringState.Create(
                    command.OrganizationId,
                    device.ZoneId.Value,
                    device.BuildingId);

                await zoneMonitoringStateRepository.AddAsync(zoneState);
            }

            monitoringStateService.RecordObservation(zoneState, command.MeasuredAt);
        }

        await unitOfWork.CompleteAsync();

        var evaluation = await riskDetectionContextFacade.EvaluateMeasurementAsync(
            command.OrganizationId,
            measurement.Id,
            command.DeviceId,
            variableType,
            command.Value,
            command.MeasuredAt);

        if (zoneState is not null && evaluation.RiskDetectionIds.Count > 0)
        {
            monitoringStateService.MarkRisk(
                zoneState,
                evaluation.RiskDetectionIds.First(),
                "RISK_ACTIVE");

            await unitOfWork.CompleteAsync();
        }

        return (measurement, evaluation);
    }
}
