using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Queries;

public record GetDevicesQuery(Guid OrganizationId, Guid? BuildingId, Guid? ZoneId, EDeviceAdministrativeStatus? AdministrativeStatus, int Page = 0, int Size = 20);