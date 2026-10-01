namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record UpdateZoneResource(
    string? Name,
    string? Description,
    string? FloorLabel);
