namespace ResQ.API.Device_Management.Domain.Model.Queries;

public record GetDeviceByIdQuery(Guid OrganizationId, Guid DeviceId);