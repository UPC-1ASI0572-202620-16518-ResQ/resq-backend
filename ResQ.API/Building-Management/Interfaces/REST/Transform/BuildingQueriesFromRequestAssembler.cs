using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class BuildingQueriesFromRequestAssembler
{
    public static GetBuildingByIdQuery ToGetBuildingByIdQuery(
        Guid organizationId, Guid buildingId)
    {
        return new GetBuildingByIdQuery(organizationId, buildingId);
    }

    public static GetBuildingsQuery ToGetBuildingsQuery(
        Guid organizationId, LocationAdministrativeStatus? administrativeStatus,
        int page, int size)
    {
        return new GetBuildingsQuery(organizationId, administrativeStatus, page, size);
    }

    public static GetZonesByBuildingIdQuery ToGetZonesByBuildingIdQuery(
        Guid organizationId, Guid buildingId,
        LocationAdministrativeStatus? administrativeStatus,
        int page, int size)
    {
        return new GetZonesByBuildingIdQuery(
            organizationId, buildingId, administrativeStatus, page, size);
    }

    public static GetZoneByIdQuery ToGetZoneByIdQuery(
        Guid organizationId, Guid buildingId, Guid zoneId)
    {
        return new GetZoneByIdQuery(organizationId, buildingId, zoneId);
    }
}
