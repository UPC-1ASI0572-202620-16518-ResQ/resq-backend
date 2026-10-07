using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.Tests.Building_Management.UnitTests;

public class BuildingCodeTests
{
    [Fact]
    public void Create_ShouldCreateValidBuildingCode()
    {
        // Act
        var buildingCode = BuildingCode.Create("BLD-CENTRAL-01");

        // Assert
        Assert.Equal("BLD-CENTRAL-01", buildingCode.Value);
    }

    [Fact]
    public void Create_ShouldNormalizeValueToUpperCase()
    {
        // Act
        var buildingCode = BuildingCode.Create("bld-central-01");

        // Assert
        Assert.Equal("BLD-CENTRAL-01", buildingCode.Value);
    }

    [Fact]
    public void Create_ShouldTrimOuterSpaces()
    {
        // Act
        var buildingCode = BuildingCode.Create("   BLD-CENTRAL-01   ");

        // Assert
        Assert.Equal("BLD-CENTRAL-01", buildingCode.Value);
    }

    [Fact]
    public void Create_ShouldAllowNumbers()
    {
        // Act
        var buildingCode = BuildingCode.Create("BLD-2026-01");

        // Assert
        Assert.Equal("BLD-2026-01", buildingCode.Value);
    }

    [Fact]
    public void Create_ShouldAllowHyphens()
    {
        // Act
        var buildingCode = BuildingCode.Create("BLD-CENTRAL-01");

        // Assert
        Assert.Equal("BLD-CENTRAL-01", buildingCode.Value);
    }

    [Fact]
    public void Create_ShouldAllowUnderscores()
    {
        // Act
        var buildingCode = BuildingCode.Create("BLD_CENTRAL_01");

        // Assert
        Assert.Equal("BLD_CENTRAL_01", buildingCode.Value);
    }

    [Fact]
    public void Create_ShouldRejectEmptyValue()
    {
        // Act
        var action = () => BuildingCode.Create("");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectWhitespaceValue()
    {
        // Act
        var action = () => BuildingCode.Create("   ");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectValueLongerThan64Characters()
    {
        // Arrange
        var value = new string('A', 65);

        // Act
        var action = () => BuildingCode.Create(value);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectSpacesInsideValue()
    {
        // Act
        var action = () => BuildingCode.Create("BLD CENTRAL 01");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_ShouldRejectSpecialCharacters()
    {
        // Act
        var action = () => BuildingCode.Create("BLD-CENTRAL!");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}