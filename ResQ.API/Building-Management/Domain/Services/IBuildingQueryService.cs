using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.Queries;

namespace ResQ.API.Building_Management.Domain.Services;

public interface IBuildingQueryService
{
    Task<Building?> Handle(GetBuildingByIdQuery query);
    Task<PagedResult<Building>> Handle(GetBuildingsQuery query);
    Task<PagedResult<Zone>> Handle(GetZonesByBuildingIdQuery query);
    Task<Zone?> Handle(GetZoneByIdQuery query);
}
