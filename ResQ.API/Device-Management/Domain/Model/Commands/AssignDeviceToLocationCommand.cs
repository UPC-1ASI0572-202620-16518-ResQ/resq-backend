namespace ResQ.API.Device_Management.Domain.Model.Commands;

public record AssignDeviceToLocationCommand(Guid OrganizationId, Guid DeviceId, Guid BuildingId, Guid? ZoneId, long ExpectedVersion);