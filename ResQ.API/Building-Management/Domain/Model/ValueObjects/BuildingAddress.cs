namespace ResQ.API.Building_Management.Domain.Model.ValueObjects;

/// <summary>
/// Physical address of a building.
/// </summary>
public record BuildingAddress
{
    public string StreetAddress { get; init; }
    public string District { get; init; }
    public string City { get; init; }
    public string CountryCode { get; init; }

    public BuildingAddress(string streetAddress, string district, string city, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(streetAddress))
            throw new ArgumentException("Street address is required.", nameof(streetAddress));
        if (streetAddress.Trim().Length > 200)
            throw new ArgumentException("Street address cannot exceed 200 characters.", nameof(streetAddress));

        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District is required.", nameof(district));
        if (district.Trim().Length > 100)
            throw new ArgumentException("District cannot exceed 100 characters.", nameof(district));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (city.Trim().Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters.", nameof(city));

        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required.", nameof(countryCode));

        var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        if (normalizedCountryCode.Length != 2 || !normalizedCountryCode.All(char.IsAsciiLetterUpper))
            throw new ArgumentException("Country code must be exactly two uppercase ASCII letters (e.g. PE).", nameof(countryCode));

        StreetAddress = streetAddress.Trim();
        District = district.Trim();
        City = city.Trim();
        CountryCode = normalizedCountryCode;
    }
}
