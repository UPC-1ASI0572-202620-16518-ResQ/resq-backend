using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record ZoneResource(
    Guid Id,
    Guid BuildingId,
    string ZoneCode,
    string Name,
    string? Description,
    string? FloorLabel,
    LocationAdministrativeStatus AdministrativeStatus,
    bool AvailableForAssignment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
