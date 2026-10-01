namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record UpdateBuildingDetailsCommand(
    Guid OrganizationId,
    Guid BuildingId,
    string? Name,
    string? Description,
    string? StreetAddress,
    string? District,
    string? City,
    string? CountryCode);
