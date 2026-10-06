using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Interfaces.ACL;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Application.Internal.CommandServices;

public class AlertCommandService(
    IAlertRepository alertRepository,
    IResponseExecutionRepository responseExecutionRepository,
    IBuildingsContextFacade buildingsContextFacade,
    IDevicesContextFacade devicesContextFacade,
    IUnitOfWork unitOfWork)
    : IAlertCommandService
{
    public async Task<Alert?> Handle(GenerateAlertCommand command)
    {
        var context = AlertContext.Create(
            command.RiskDetectionId,
            command.RiskTypeCode,
            command.SeverityCode,
            command.BuildingId,
            command.ZoneId,
            command.DetectedAt);

        // The location is validated through the Building Management ACL
        if (context.BuildingId.HasValue)
        {
            var validLocation = await buildingsContextFacade.ValidateAssignmentAsync(
                command.OrganizationId, context.BuildingId.Value, context.ZoneId);

            if (!validLocation)
                throw new InvalidOperationException("The building or zone does not exist or is not active.");
        }

        // Each response action must target an active device with that actuation capability (Device Management ACL)
        foreach (var action in command.ResponseActions)
        {
            await ValidateResponseActionTargetAsync(command.OrganizationId, action);
        }

        var alert = Alert.Generate(command.OrganizationId, context, command.Recipients);

        await alertRepository.AddAsync(alert);

        foreach (var action in command.ResponseActions)
        {
            var execution = ResponseExecution.Request(command.OrganizationId, alert.Id, context.RiskDetectionId, action);
            await responseExecutionRepository.AddAsync(execution);
        }

        await unitOfWork.CompleteAsync();

        return alert;
    }

    private async Task ValidateResponseActionTargetAsync(Guid organizationId, ResponseActionSnapshot action)
    {
        var device = await devicesContextFacade.GetDeviceCatalogEntry(organizationId, action.TargetDeviceId);

        if (device is null)
            throw new InvalidOperationException($"Target device '{action.TargetDeviceId}' does not exist.");

        if (device.AdministrativeStatus != EDeviceAdministrativeStatus.Active)
            throw new InvalidOperationException($"Target device '{device.DeviceCode}' is not active.");

        var hasCapability = device.Capabilities.Any(capability =>
            capability.Kind == ECapabilityKind.Actuation &&
            string.Equals(capability.Code, action.TargetCapabilityCode, StringComparison.OrdinalIgnoreCase));

        if (!hasCapability)
            throw new InvalidOperationException(
                $"Target device '{device.DeviceCode}' has no actuation capability '{action.TargetCapabilityCode}'.");
    }
}
