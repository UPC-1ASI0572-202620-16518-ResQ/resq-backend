namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record RegisterBuildingResource(
    string BuildingCode,
    string Name,
    string? Description,
    BuildingAddressResource Address,
    IReadOnlyCollection<FloorDefinitionResource>? Floors = null);

public record FloorDefinitionResource(string Label, int Level);
