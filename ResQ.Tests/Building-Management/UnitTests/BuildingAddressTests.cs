using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.Tests.Building_Management.UnitTests;

public class BuildingAddressTests
{
    [Fact]
    public void Constructor_ShouldCreateValidAddress()
    {
        // Act
        var address = new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "Lima",
            "PE");

        // Assert
        Assert.Equal("Av. Javier Prado Este 456", address.StreetAddress);
        Assert.Equal("San Isidro", address.District);
        Assert.Equal("Lima", address.City);
        Assert.Equal("PE", address.CountryCode);
    }

    [Fact]
    public void Constructor_ShouldNormalizeCountryCodeToUpperCase()
    {
        // Act
        var address = new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "Lima",
            "pe");

        // Assert
        Assert.Equal("PE", address.CountryCode);
    }

    [Fact]
    public void Constructor_ShouldTrimStreetAddress()
    {
        // Act
        var address = new BuildingAddress(
            "   Av. Javier Prado Este 456   ",
            "San Isidro",
            "Lima",
            "PE");

        // Assert
        Assert.Equal(
            "Av. Javier Prado Este 456",
            address.StreetAddress);
    }

    [Fact]
    public void Constructor_ShouldTrimDistrict()
    {
        // Act
        var address = new BuildingAddress(
            "Av. Javier Prado Este 456",
            "   San Isidro   ",
            "Lima",
            "PE");

        // Assert
        Assert.Equal("San Isidro", address.District);
    }

    [Fact]
    public void Constructor_ShouldTrimCity()
    {
        // Act
        var address = new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "   Lima   ",
            "PE");

        // Assert
        Assert.Equal("Lima", address.City);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyStreetAddress()
    {
        // Act
        var action = () => new BuildingAddress(
            "",
            "San Isidro",
            "Lima",
            "PE");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyDistrict()
    {
        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            "",
            "Lima",
            "PE");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyCity()
    {
        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "",
            "PE");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyCountryCode()
    {
        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "Lima",
            "");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectStreetAddressLongerThan200Characters()
    {
        // Arrange
        var streetAddress = new string('A', 201);

        // Act
        var action = () => new BuildingAddress(
            streetAddress,
            "San Isidro",
            "Lima",
            "PE");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectDistrictLongerThan100Characters()
    {
        // Arrange
        var district = new string('A', 101);

        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            district,
            "Lima",
            "PE");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectCityLongerThan100Characters()
    {
        // Arrange
        var city = new string('A', 101);

        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            city,
            "PE");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectCountryCodeWithMoreThanTwoCharacters()
    {
        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "Lima",
            "PER");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_ShouldRejectCountryCodeWithLessThanTwoCharacters()
    {
        // Act
        var action = () => new BuildingAddress(
            "Av. Javier Prado Este 456",
            "San Isidro",
            "Lima",
            "P");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}