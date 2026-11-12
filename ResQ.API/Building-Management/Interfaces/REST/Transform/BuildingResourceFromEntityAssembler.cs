using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Interfaces.REST.Resources;

namespace ResQ.API.Building_Management.Interfaces.REST.Transform;

public static class BuildingResourceFromEntityAssembler
{
    public static BuildingResource ToResourceFromEntity(Building entity)
    {
        var address = new BuildingAddressResource(
            entity.Address.StreetAddress,
            entity.Address.District,
            entity.Address.City,
            entity.Address.CountryCode);

        var zones = entity.Zones
            .Select(z => ZoneResourceFromEntityAssembler.ToResourceFromEntity(z, entity))
            .ToList();
        var floors = entity.Floors.Select(floor => new FloorResource(
            floor.Id, floor.BuildingId, floor.Label, floor.Level, floor.CreatedAt, floor.UpdatedAt)).ToList();

        return new BuildingResource(
            entity.Id,
            entity.OrganizationId,
            entity.BuildingCode.Value,
            entity.Name,
            entity.Description,
            address,
            entity.AdministrativeStatus,
            floors,
            zones,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Version);
    }
}
