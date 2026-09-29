using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Commands;

namespace ResQ.API.Device_Management.Domain.Services;

public interface IDeviceCommandService
{
    Task<Device?> Handle(RegisterDeviceCommand command);

    Task<Device?> Handle(UpdateDeviceDetailsCommand command);

    Task<Device?> Handle(ReplaceDeviceCapabilitiesCommand command);

    Task<Device?> Handle(AssignDeviceToLocationCommand command);

    Task<Device?> Handle(ChangeDeviceAdministrativeStatusCommand command);
}