namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record AddZoneToBuildingResource(
    string ZoneCode,
    string Name,
    string? Description,
    string? FloorLabel);
