using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Commands;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Domain.Repositories;
using ResQ.API.Device_Management.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Device_Management.Application.Internal.CommandServices;

public class DeviceCommandService(
    IDeviceRepository deviceRepository,
    IUnitOfWork unitOfWork,
    IBuildingsContextFacade buildingsContextFacade) : IDeviceCommandService
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

        var isLocationValid = await buildingsContextFacade.ValidateAssignmentAsync(
            command.OrganizationId, command.Assignment.BuildingId, command.Assignment.ZoneId);

        if (!isLocationValid)
        {
            throw new InvalidOperationException("The specified building or zone does not exist or is not active for device assignment.");
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

        var newName = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Name)
            ? device.Name
            : command.Name!.Trim();

        var newDescription = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Description)
            ? device.Description
            : command.Description?.Trim();

        var existingSpecs = device.Specifications;
        var newManufacturer = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Manufacturer)
            ? existingSpecs.Manufacturer
            : command.Manufacturer!.Trim();

        var newModel = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Model)
            ? existingSpecs.Model
            : command.Model!.Trim();

        var newSerialNumber = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.SerialNumber)
            ? existingSpecs.SerialNumber
            : command.SerialNumber!.Trim();

        var newSpecs = new DeviceSpecifications(newManufacturer, newModel, newSerialNumber);

        device.UpdateDetails(newName, newDescription, newSpecs);

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

        if (command.Capabilities == null || command.Capabilities.Count == 0)
        {
            return device;
        }

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

        var targetBuildingId = command.BuildingId == Guid.Empty ? device.Assignment.BuildingId : command.BuildingId;
        var targetZoneId = command.ZoneId == Guid.Empty ? device.Assignment.ZoneId : command.ZoneId;

        var isLocationValid = await buildingsContextFacade.ValidateAssignmentAsync(
            command.OrganizationId, targetBuildingId, targetZoneId);

        if (!isLocationValid)
        {
            throw new InvalidOperationException("The specified building or zone does not exist or is not active for device assignment.");
        }

        var assignment = new DeviceAssignment(targetBuildingId, targetZoneId);

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

        device.ChangeAdministrativeStatus(command.AdministrativeStatus);

        deviceRepository.Update(device);
        await unitOfWork.CompleteAsync();

        return device;
    }
}