using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using Xunit;
using ResQ.API.Building_Management.Interfaces.REST.Resources;
using ResQ.API.Building_Management.Interfaces.REST.Transform;

namespace ResQ.API.Tests;

public sealed class BuildingFloorRegressionTests
{
    [Fact]
    public void EmptyFloorsRemainIndependentAndOrdered()
    {
        var building = NewBuilding();
        var third = building.AddFloor("Floor 3", 3);
        var first = building.AddFloor("Floor 1", 1);
        var second = building.AddFloor("Floor 2", 2);

        Assert.Empty(building.Zones);
        Assert.Equal(new[] { first.Id, second.Id, third.Id }, building.Floors.Select(floor => floor.Id));
        Assert.Equal(new[] { 1, 2, 3 }, building.Floors.Select(floor => floor.Level));
    }

    [Fact]
    public void FloorDeletionIsRejectedWhileAZoneReferencesIt()
    {
        var building = NewBuilding();
        var floor = building.AddFloor("Floor 1", 1);
        building.AddZone("LAB_101", "Laboratory", null, floor.Label);

        var error = Assert.Throws<InvalidOperationException>(() => building.RemoveFloor(floor.Id));

        Assert.Contains("zones", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(building.Floors);
        Assert.Single(building.Zones);
    }

    [Fact]
    public void RegistrationContractCarriesThreeEmptyFloorsToTheCommand()
    {
        var resource = new RegisterBuildingResource(
            "BLDG_THREE",
            "Three floor building",
            null,
            new BuildingAddressResource("Street", "District", "Lima", "PE"),
            [
                new FloorDefinitionResource("Floor 1", 1),
                new FloorDefinitionResource("Floor 2", 2),
                new FloorDefinitionResource("Floor 3", 3),
            ]);

        var command = RegisterBuildingCommandFromResourceAssembler.ToCommandFromResource(Guid.NewGuid(), resource);

        Assert.Equal(3, command.Floors?.Count);
        Assert.Equal(new[] { "Floor 1", "Floor 2", "Floor 3" }, command.Floors?.Select(floor => floor.Label));
    }

    [Fact]
    public void RemoteFloorPlanVersionSurvivesUpdates()
    {
        var plan = FloorPlanLayout.Create(Guid.NewGuid(), Guid.NewGuid(), "Floor 1", "{\"elements\":[]}", 10);
        Assert.Equal(1, plan.Version);

        plan.Update("Floor 1", "{\"elements\":[{\"id\":\"area-1\"}]}", 11);

        Assert.Equal(2, plan.Version);
        Assert.Equal(11, plan.UpdatedByUserId);
        Assert.Contains("area-1", plan.LayoutJson);
    }

    private static Building NewBuilding() => Building.Register(
        Guid.NewGuid(),
        BuildingCode.Create("BLDG_TEST"),
        "Regression building",
        null,
        new BuildingAddress("Test street", "Test district", "Lima", "PE"));
}
