namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record BuildingAddressResource(
    string? StreetAddress,
    string? District,
    string? City,
    string? CountryCode);
