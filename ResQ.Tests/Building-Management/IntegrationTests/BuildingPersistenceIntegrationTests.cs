using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.Tests.Building_Management.IntegrationTests;

public class BuildingPersistenceIntegrationTests
{
    private static async Task<(SqliteConnection Connection, AppDbContext Context)> CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return (connection, context);
    }

    private static Building CreateBuilding(
        Guid organizationId,
        string code = "BLD-CENTRAL-01")
    {
        return Building.Register(
            organizationId,
            BuildingCode.Create(code),
            "Edificio Central",
            "Edificación principal de la organización",
            new BuildingAddress(
                "Av. Javier Prado Este 456",
                "San Isidro",
                "Lima",
                "PE"));
    }

    [Fact]
    public async Task ShouldPersistBuildingAndRetrieveItFromDatabase()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var building = CreateBuilding(organizationId);

        // Act
        context.Set<Building>().Add(building);

        await context.SaveChangesAsync();

        await using var verificationContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options);

        var persistedBuilding = await verificationContext
            .Set<Building>()
            .SingleAsync(b => b.Id == building.Id);

        // Assert
        Assert.NotNull(persistedBuilding);
        Assert.Equal(building.Id, persistedBuilding.Id);
        Assert.Equal(organizationId, persistedBuilding.OrganizationId);
        Assert.Equal("BLD-CENTRAL-01", persistedBuilding.BuildingCode.Value);
        Assert.Equal("Edificio Central", persistedBuilding.Name);
        Assert.Equal(
            "Edificación principal de la organización",
            persistedBuilding.Description);

        Assert.Equal(
            "Av. Javier Prado Este 456",
            persistedBuilding.Address.StreetAddress);

        Assert.Equal(
            "San Isidro",
            persistedBuilding.Address.District);

        Assert.Equal(
            "Lima",
            persistedBuilding.Address.City);

        Assert.Equal(
            "PE",
            persistedBuilding.Address.CountryCode);
    }

    [Fact]
    public async Task ShouldPersistBuildingWithItsZones()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var building = CreateBuilding(organizationId);

        var zone = building.AddZone(
            "ZON-SERVER-01",
            "Sala de Servidores",
            "Data center principal",
            "Sótano 1");

        // Act
        context.Set<Building>().Add(building);

        await context.SaveChangesAsync();

        await using var verificationContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options);

        var persistedBuilding = await verificationContext
            .Set<Building>()
            .Include(b => b.Zones)
            .SingleAsync(b => b.Id == building.Id);

        // Assert
        Assert.Single(persistedBuilding.Zones);

        var persistedZone = persistedBuilding.Zones.Single();

        Assert.Equal(zone.Id, persistedZone.Id);
        Assert.Equal(building.Id, persistedZone.BuildingId);
        Assert.Equal("ZON-SERVER-01", persistedZone.ZoneCode.Value);
        Assert.Equal("Sala de Servidores", persistedZone.Name);
        Assert.Equal("Data center principal", persistedZone.Description);
        Assert.Equal("Sótano 1", persistedZone.FloorLabel);
    }

    [Fact]
    public async Task ShouldPersistBuildingAndZoneInTheSameTransaction()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var building = CreateBuilding(organizationId);

        building.AddZone(
            "ZON-OPERATIONS-01",
            "Centro de Operaciones",
            "Zona de monitoreo",
            "Piso 1");

        // Act
        context.Set<Building>().Add(building);

        await context.SaveChangesAsync();

        await using var verificationContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options);

        var persistedBuilding = await verificationContext
            .Set<Building>()
            .Include(b => b.Zones)
            .SingleAsync(b => b.Id == building.Id);

        // Assert
        Assert.NotNull(persistedBuilding);
        Assert.Single(persistedBuilding.Zones);

        Assert.Equal(
            building.Id,
            persistedBuilding.Zones.Single().BuildingId);
    }

    [Fact]
    public async Task ShouldAllowSameBuildingCodeForDifferentOrganizations()
    {
        // Arrange
        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var firstOrganizationId = Guid.NewGuid();
        var secondOrganizationId = Guid.NewGuid();

        var firstBuilding = CreateBuilding(
            firstOrganizationId,
            "BLD-CENTRAL-01");

        var secondBuilding = CreateBuilding(
            secondOrganizationId,
            "BLD-CENTRAL-01");

        // Act
        context.Set<Building>().Add(firstBuilding);
        context.Set<Building>().Add(secondBuilding);

        await context.SaveChangesAsync();

        var persistedFirstBuilding = await context
            .Set<Building>()
            .SingleAsync(b => b.OrganizationId == firstOrganizationId);

        var persistedSecondBuilding = await context
            .Set<Building>()
            .SingleAsync(b => b.OrganizationId == secondOrganizationId);

        // Assert
        Assert.NotNull(persistedFirstBuilding);
        Assert.NotNull(persistedSecondBuilding);

        Assert.Equal(
            "BLD-CENTRAL-01",
            persistedFirstBuilding.BuildingCode.Value);

        Assert.Equal(
            "BLD-CENTRAL-01",
            persistedSecondBuilding.BuildingCode.Value);

        Assert.Equal(
            firstOrganizationId,
            persistedFirstBuilding.OrganizationId);

        Assert.Equal(
            secondOrganizationId,
            persistedSecondBuilding.OrganizationId);
    }

    [Fact]
    public async Task ShouldRejectDuplicateBuildingCodeWithinSameOrganization()
    {
        // Arrange
        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var organizationId = Guid.NewGuid();

        var firstBuilding = CreateBuilding(
            organizationId,
            "BLD-CENTRAL-01");

        var secondBuilding = CreateBuilding(
            organizationId,
            "BLD-CENTRAL-01");

        context.Set<Building>().Add(firstBuilding);

        await context.SaveChangesAsync();

        // Act
        context.Set<Building>().Add(secondBuilding);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());
    }
}