using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Commands;

public record ChangeDeviceAdministrativeStatusCommand(Guid OrganizationId, Guid DeviceId, EDeviceAdministrativeStatus AdministrativeStatus, long ExpectedVersion);