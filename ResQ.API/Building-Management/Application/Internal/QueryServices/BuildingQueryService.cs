using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Domain.Repositories;
using ResQ.API.Building_Management.Domain.Services;

namespace ResQ.API.Building_Management.Application.Internal.QueryServices;

public class BuildingQueryService(IBuildingRepository buildingRepository) : IBuildingQueryService
{
    public async Task<Building?> Handle(GetBuildingByIdQuery query)
    {
        return await buildingRepository.FindByIdAndOrganizationIdAsync(query.BuildingId, query.OrganizationId);
    }

    public async Task<PagedResult<Building>> Handle(GetBuildingsQuery query)
    {
        return await buildingRepository.FindPageAsync(
            query.OrganizationId,
            query.AdministrativeStatus,
            query.Page,
            query.Size);
    }

    public async Task<PagedResult<Zone>> Handle(GetZonesByBuildingIdQuery query)
    {
        return await buildingRepository.FindZonesPageAsync(
            query.OrganizationId,
            query.BuildingId,
            query.AdministrativeStatus,
            query.Page,
            query.Size);
    }

    public async Task<Zone?> Handle(GetZoneByIdQuery query)
    {
        return await buildingRepository.FindZoneByIdAsync(
            query.OrganizationId,
            query.BuildingId,
            query.ZoneId);
    }

    public async Task<Building?> Handle(GetBuildingByZoneIdQuery query)
    {
        return await buildingRepository.FindByZoneIdAndOrganizationIdAsync(query.ZoneId, query.OrganizationId);
    }
}
