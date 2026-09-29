using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Domain.Repositories;
using ResQ.API.Device_Management.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Device_Management.Application.Internal.CommandServices;

public class DeviceCommandService(IDeviceRepository deviceRepository, IUnitOfWork unitOfWork) : IDeviceCommandService
{
   public async Task<Device?> Handle(RegisterDeviceCommand command)
    {
        var deviceCode = DeviceCode.Create(command.DeviceCode);
        var deviceCodeExists = await deviceRepository.ExistsByOrganizationIdAndDeviceCodeAsync(command.OrganizationId, deviceCode);

        if (deviceCodeExists)
        {
            throw new InvalidOperationException("A device with this code already exists in the organization.");
        }

        if (command.ExternalReference != null)
        {
            var externalReferenceExists =
                await deviceRepository.ExistsByExternalReferenceAsync(
                    command.OrganizationId,
                    command.ExternalReference.SourceSystem,
                    command.ExternalReference.ExternalDeviceId);

            if (externalReferenceExists)
            {
                throw new InvalidOperationException("A device with this external reference already exists.");
            }
        }

        var device = Device.Register(
            command.OrganizationId,
            deviceCode,
            command.Name,
            command.Description,
            command.Specifications,
            command.Assignment,
            command.ExternalReference,
            command.Capabilities);

        await deviceRepository.AddAsync(device);
        await unitOfWork.CompleteAsync();

        return device;
    }

    public async Task<Device?> Handle(UpdateDeviceDetailsCommand command)
    {
        var device = await deviceRepository.FindByIdAndOrganizationIdAsync(command.DeviceId, command.OrganizationId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found.");
        }

        ValidateVersion(device, command.ExpectedVersion);

        device.UpdateDetails(command.Name, command.Description, command.Specifications);

        deviceRepository.Update(device);
        await unitOfWork.CompleteAsync();

        return device;
    }

    public async Task<Device?> Handle(ReplaceDeviceCapabilitiesCommand command)
    {
        var device = await deviceRepository.FindByIdAndOrganizationIdAsync(command.DeviceId, command.OrganizationId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found.");
        }

        ValidateVersion(device, command.ExpectedVersion);

        device.ReplaceCapabilities(command.Capabilities);

        deviceRepository.Update(device);
        await unitOfWork.CompleteAsync();

        return device;
    }

    public async Task<Device?> Handle(
        AssignDeviceToLocationCommand command)
    {
        var device = await deviceRepository.FindByIdAndOrganizationIdAsync(command.DeviceId, command.OrganizationId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found.");
        }

        ValidateVersion(device, command.ExpectedVersion);

        var assignment = new DeviceAssignment(command.BuildingId, command.ZoneId);

        device.AssignTo(assignment);

        deviceRepository.Update(device);
        await unitOfWork.CompleteAsync();

        return device;
    }

    public async Task<Device?> Handle(ChangeDeviceAdministrativeStatusCommand command)
    {
        var device = await deviceRepository.FindByIdAndOrganizationIdAsync(command.DeviceId, command.OrganizationId);

        if (device == null)
        {
            throw new KeyNotFoundException("Device not found.");
        }

        ValidateVersion(device, command.ExpectedVersion);

        device.ChangeAdministrativeStatus(command.AdministrativeStatus);

        deviceRepository.Update(device);
        await unitOfWork.CompleteAsync();

        return device;
    }

    private static void ValidateVersion(Device device, long expectedVersion)
    {
        if (device.Version != expectedVersion)
        {
            throw new InvalidOperationException("The device was modified by another operation.");
        }
    }
}