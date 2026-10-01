using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Queries;
using ResQ.API.Device_Management.Domain.Repositories;
using ResQ.API.Device_Management.Domain.Services;

namespace ResQ.API.Device_Management.Application.Internal.QueryServices;

public class DeviceQueryService(IDeviceRepository deviceRepository) : IDeviceQueryService
{
    public async Task<Device?> Handle(GetDeviceByIdQuery query)
    {
        return await deviceRepository.FindByIdAndOrganizationIdAsync(query.DeviceId, query.OrganizationId);
    }

    public async Task<PagedResult<Device>> Handle(GetDevicesQuery query)
    {
        return await deviceRepository.FindPageAsync(
            query.OrganizationId,
            query.BuildingId,
            query.ZoneId,
            query.AdministrativeStatus,
            query.Page,
            query.Size);
    }

    public async Task<Device?> Handle(GetDeviceByExternalReferenceQuery query)
    {
        return await deviceRepository.FindByExternalReferenceAsync(
            query.OrganizationId,
            query.SourceSystem,
            query.ExternalDeviceId);
    }
}