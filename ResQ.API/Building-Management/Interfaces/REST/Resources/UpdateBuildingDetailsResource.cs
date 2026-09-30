namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record UpdateBuildingDetailsResource(
    string? Name,
    string? Description,
    BuildingAddressResource? Address);
