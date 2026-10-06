using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Building_Management.Domain.Repositories;

/// <summary>
/// Repository contract for Building aggregates.
/// </summary>
public interface IBuildingRepository : IBaseRepository<Building>
{
    Task<Building?> FindByIdAndOrganizationIdAsync(Guid buildingId, Guid organizationId);

    Task<bool> ExistsByOrganizationIdAndBuildingCodeAsync(Guid organizationId, BuildingCode buildingCode);

    Task<PagedResult<Building>> FindPageAsync(
        Guid organizationId,
        LocationAdministrativeStatus? administrativeStatus,
        int page, int size);

    Task<PagedResult<Zone>> FindZonesPageAsync(
        Guid organizationId,
        Guid buildingId,
        LocationAdministrativeStatus? administrativeStatus,
        int page, int size);

    Task<Zone?> FindZoneByIdAsync(Guid organizationId, Guid buildingId, Guid zoneId);

    Task<Building?> FindByZoneIdAndOrganizationIdAsync(Guid zoneId, Guid organizationId);
}
