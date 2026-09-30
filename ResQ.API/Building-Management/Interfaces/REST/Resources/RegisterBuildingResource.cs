namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record RegisterBuildingResource(
    string BuildingCode,
    string Name,
    string? Description,
    BuildingAddressResource Address);
