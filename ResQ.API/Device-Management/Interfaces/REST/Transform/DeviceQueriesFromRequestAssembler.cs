using ResQ.API.Device_Management.Domain.Model.Queries;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Interfaces.REST.Transform;

public static class DeviceQueriesFromRequestAssembler
{
    public static GetDeviceByIdQuery ToGetDeviceByIdQuery(Guid organizationId, Guid deviceId)
    {
        return new GetDeviceByIdQuery(organizationId, deviceId);
    }

    public static GetDevicesQuery ToGetDevicesQuery(Guid organizationId, Guid? buildingId,
        Guid? zoneId, EDeviceAdministrativeStatus? administrativeStatus, int page, int size)
    {
        return new GetDevicesQuery(
            organizationId,
            buildingId,
            zoneId,
            administrativeStatus,
            page,
            size);
    }

    public static GetDeviceByExternalReferenceQuery
        ToGetDeviceByExternalReferenceQuery(Guid organizationId, string sourceSystem, string externalDeviceId)
    {
        return new GetDeviceByExternalReferenceQuery(
            organizationId,
            sourceSystem,
            externalDeviceId);
    }
}