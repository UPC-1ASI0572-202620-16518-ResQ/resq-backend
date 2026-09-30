using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Building_Management.Interfaces.REST.Resources;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class ZoneResourceFromEntityAssembler
{
    public static ZoneResource ToResourceFromEntity(Zone zone, Building building)
    {
        var availableForAssignment =
            building.AdministrativeStatus == LocationAdministrativeStatus.Active &&
            zone.AdministrativeStatus == LocationAdministrativeStatus.Active;

        return new ZoneResource(
            zone.Id,
            building.Id,
            zone.ZoneCode.Value,
            zone.Name,
            zone.Description,
            zone.FloorLabel,
            zone.AdministrativeStatus,
            availableForAssignment,
            zone.CreatedAt,
            zone.UpdatedAt);
    }
}
