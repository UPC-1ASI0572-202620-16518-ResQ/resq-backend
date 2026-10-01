using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record BuildingResource(
    Guid Id,
    Guid OrganizationId,
    string BuildingCode,
    string Name,
    string? Description,
    BuildingAddressResource Address,
    LocationAdministrativeStatus AdministrativeStatus,
    List<ZoneResource> Zones,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);
