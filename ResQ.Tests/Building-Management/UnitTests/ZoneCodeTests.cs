using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.Tests.Building_Management.UnitTests;

public class ZoneCodeTests
{
    [Fact]
    public void Create_ShouldCreateValidZoneCode()
    {
        // Act
        var zoneCode = ZoneCode.Create("ZON-SERVER-01");

        // Assert
        Assert.Equal("ZON-SERVER-01", zoneCode.Value);
    }

    [Fact]
    public void Create_ShouldNormalizeValueToUpperCase()
    {
        // Act
        var zoneCode = ZoneCode.Create("zon-server-01");

        // Assert
        Assert.Equal("ZON-SERVER-01", zoneCode.Value);
    }

    [Fact]
    public void Create_ShouldTrimOuterSpaces()
    {
        // Act
        var zoneCode = ZoneCode.Create("   ZON-SERVER-01   ");

        // Assert
        Assert.Equal("ZON-SERVER-01", zoneCode.Value);
    }

    [Fact]
    public void Create_ShouldAllowNumbers()
    {
        // Act
        var zoneCode = ZoneCode.Create("ZON-2026-01");

        // Assert
        Assert.Equal("ZON-2026-01", zoneCode.Value);
    }

    [Fact]
    public void Create_ShouldAllowHyphens()
    {
        // Act
        var zoneCode = ZoneCode.Create("ZON-SERVER-01");

        // Assert
        Assert.Equal("ZON-SERVER-01", zoneCode.Value);
    }

    [Fact]
    public void Create_ShouldAllowUnderscores()
    {
        // Act
        var zoneCode = ZoneCode.Create("ZON_SERVER_01");

        // Assert
        Assert.Equal("ZON_SERVER_01", zoneCode.Value);
    }

    [Fact]
    public void Create_ShouldRejectEmptyValue()
    {
        // Act
        var action = () => ZoneCode.Create("");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectWhitespaceValue()
    {
        // Act
        var action = () => ZoneCode.Create("   ");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectValueLongerThan64Characters()
    {
        // Arrange
        var value = new string('A', 65);

        // Act
        var action = () => ZoneCode.Create(value);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectSpacesInsideValue()
    {
        // Act
        var action = () => ZoneCode.Create("ZON SERVER 01");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectSpecialCharacters()
    {
        // Act
        var action = () => ZoneCode.Create("ZON-SERVER!");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}