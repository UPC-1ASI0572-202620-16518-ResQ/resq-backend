using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Building_Management.Domain.Services;

namespace ResQ.API.Building_Management.Interfaces.ACL;

/// <summary>
/// Implementation of the Anti-Corruption Layer facade for Building Management.
/// </summary>
public class BuildingsContextFacade(IBuildingQueryService buildingQueryService) : IBuildingsContextFacade
{
    public async Task<bool> ValidateAssignmentAsync(Guid organizationId, Guid buildingId, Guid? zoneId)
    {
        var building = await buildingQueryService.Handle(new GetBuildingByIdQuery(organizationId, buildingId));

        if (building == null || building.AdministrativeStatus != LocationAdministrativeStatus.Active)
            return false;

        if (zoneId.HasValue && zoneId.Value != Guid.Empty)
        {
            var zone = building.Zones.FirstOrDefault(z => z.Id == zoneId.Value);
            if (zone == null || zone.AdministrativeStatus != LocationAdministrativeStatus.Active)
                return false;
        }

        return true;
    }

    public async Task<bool> ZoneExistsAsync(Guid organizationId, Guid zoneId)
    {
        var building = await buildingQueryService.Handle(new GetBuildingByZoneIdQuery(organizationId, zoneId));
        return building != null;
    }
}
