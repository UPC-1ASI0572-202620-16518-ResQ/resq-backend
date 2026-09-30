namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record AddZoneToBuildingCommand(
    Guid OrganizationId,
    Guid BuildingId,
    string ZoneCode,
    string Name,
    string? Description,
    string? FloorLabel);
