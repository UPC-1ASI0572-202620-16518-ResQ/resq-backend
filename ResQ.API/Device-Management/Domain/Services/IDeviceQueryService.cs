using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Queries;

namespace ResQ.API.Device_Management.Domain.Services;

public interface IDeviceQueryService
{
    Task<Device?> Handle(GetDeviceByIdQuery query);

    Task<PagedResult<Device>> Handle(GetDevicesQuery query);

    Task<Device?> Handle(GetDeviceByExternalReferenceQuery query);
}