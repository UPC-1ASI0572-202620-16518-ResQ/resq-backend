namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record UpdateZoneCommand(
    Guid OrganizationId,
    Guid BuildingId,
    Guid ZoneId,
    string? Name,
    string? Description,
    string? FloorLabel);
