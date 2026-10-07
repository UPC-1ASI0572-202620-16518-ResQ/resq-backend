using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.Tests.Building_Management.UnitTests;

public class BuildingTests
{
    private static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static BuildingCode CreateBuildingCode(string value = "BLD-CENTRAL-01")
    {
        return BuildingCode.Create(value);
    }

    private static BuildingAddress CreateAddress(
        string streetAddress = "Av. Javier Prado Este 456",
        string district = "San Isidro",
        string city = "Lima",
        string countryCode = "PE")
    {
        return new BuildingAddress(
            streetAddress,
            district,
            city,
            countryCode);
    }

    private static Building CreateBuilding(
        string name = "Edificio Central",
        string? description = "Edificio principal de operaciones")
    {
        return Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            name,
            description,
            CreateAddress());
    }


    // ============================================================
    // REGISTER
    // ============================================================

    [Fact]
    public void Register_ShouldCreateActiveBuilding()
    {
        // Arrange
        var buildingCode = CreateBuildingCode();
        var address = CreateAddress();

        // Act
        var building = Building.Register(
            OrganizationId,
            buildingCode,
            "Edificio Central",
            "Edificio principal",
            address);

        // Assert
        Assert.NotEqual(Guid.Empty, building.Id);
        Assert.Equal(OrganizationId, building.OrganizationId);
        Assert.Equal(buildingCode, building.BuildingCode);
        Assert.Equal("Edificio Central", building.Name);
        Assert.Equal("Edificio principal", building.Description);
        Assert.Equal(address, building.Address);
        Assert.Equal(LocationAdministrativeStatus.Active, building.AdministrativeStatus);
        Assert.Empty(building.Zones);
        Assert.Equal(1, building.Version);
    }


    [Fact]
    public void Register_ShouldNormalizeNameAndDescription()
    {
        // Act
        var building = Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            "   Edificio Central   ",
            "   Edificio principal   ",
            CreateAddress());

        // Assert
        Assert.Equal("Edificio Central", building.Name);
        Assert.Equal("Edificio principal", building.Description);
    }


    [Fact]
    public void Register_ShouldNormalizeEmptyDescriptionToNull()
    {
        // Act
        var building = Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            "Edificio Central",
            "   ",
            CreateAddress());

        // Assert
        Assert.Null(building.Description);
    }


    [Fact]
    public void Register_ShouldRejectEmptyOrganizationId()
    {
        // Act
        var action = () => Building.Register(
            Guid.Empty,
            CreateBuildingCode(),
            "Edificio Central",
            "Edificio principal",
            CreateAddress());

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Register_ShouldRejectEmptyName()
    {
        // Act
        var action = () => Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            "",
            "Edificio principal",
            CreateAddress());

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Register_ShouldRejectNameLongerThan120Characters()
    {
        // Arrange
        var name = new string('A', 121);

        // Act
        var action = () => Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            name,
            "Edificio principal",
            CreateAddress());

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Register_ShouldRejectDescriptionLongerThan500Characters()
    {
        // Arrange
        var description = new string('A', 501);

        // Act
        var action = () => Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            "Edificio Central",
            description,
            CreateAddress());

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Register_ShouldRejectNullBuildingCode()
    {
        // Act
        var action = () => Building.Register(
            OrganizationId,
            null!,
            "Edificio Central",
            "Edificio principal",
            CreateAddress());

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }


    [Fact]
    public void Register_ShouldRejectNullAddress()
    {
        // Act
        var action = () => Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            "Edificio Central",
            "Edificio principal",
            null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }


    // ============================================================
    // UPDATE DETAILS
    // ============================================================

    [Fact]
    public void UpdateDetails_ShouldUpdateBuildingInformation()
    {
        // Arrange
        var building = CreateBuilding();
        var newAddress = CreateAddress(
            "Av. Arequipa 100",
            "Lince",
            "Lima",
            "PE");

        var initialVersion = building.Version;

        // Act
        building.UpdateDetails(
            "Edificio Actualizado",
            "Nueva descripción",
            newAddress);

        // Assert
        Assert.Equal("Edificio Actualizado", building.Name);
        Assert.Equal("Nueva descripción", building.Description);
        Assert.Equal(newAddress, building.Address);
        Assert.Equal(initialVersion + 1, building.Version);
    }


    [Fact]
    public void UpdateDetails_ShouldNormalizeNameAndDescription()
    {
        // Arrange
        var building = CreateBuilding();

        // Act
        building.UpdateDetails(
            "   Edificio Actualizado   ",
            "   Nueva descripción   ",
            CreateAddress());

        // Assert
        Assert.Equal("Edificio Actualizado", building.Name);
        Assert.Equal("Nueva descripción", building.Description);
    }


    [Fact]
    public void UpdateDetails_ShouldNormalizeEmptyDescriptionToNull()
    {
        // Arrange
        var building = CreateBuilding();

        // Act
        building.UpdateDetails(
            "Edificio Actualizado",
            "   ",
            CreateAddress());

        // Assert
        Assert.Null(building.Description);
    }


    [Fact]
    public void UpdateDetails_ShouldRejectEmptyName()
    {
        // Arrange
        var building = CreateBuilding();

        // Act
        var action = () => building.UpdateDetails(
            "",
            "Nueva descripción",
            CreateAddress());

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void UpdateDetails_ShouldRejectNullAddress()
    {
        // Arrange
        var building = CreateBuilding();

        // Act
        var action = () => building.UpdateDetails(
            "Edificio Actualizado",
            "Nueva descripción",
            null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }


    [Fact]
    public void UpdateDetails_ShouldNotChangeVersionWhenNothingChanges()
    {
        // Arrange
        var address = CreateAddress();

        var building = Building.Register(
            OrganizationId,
            CreateBuildingCode(),
            "Edificio Central",
            "Edificio principal",
            address);

        var initialVersion = building.Version;

        // Act
        building.UpdateDetails(
            "Edificio Central",
            "Edificio principal",
            address);

        // Assert
        Assert.Equal(initialVersion, building.Version);
    }


    // ============================================================
    // ADMINISTRATIVE STATUS
    // ============================================================

    [Fact]
    public void ChangeAdministrativeStatus_ShouldDeactivateBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        var initialVersion = building.Version;

        // Act
        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Inactive);

        // Assert
        Assert.Equal(
            LocationAdministrativeStatus.Inactive,
            building.AdministrativeStatus);

        Assert.Equal(initialVersion + 1, building.Version);
    }


    [Fact]
    public void ChangeAdministrativeStatus_ShouldReactivateBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Inactive);

        var versionBeforeReactivation = building.Version;

        // Act
        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Active);

        // Assert
        Assert.Equal(
            LocationAdministrativeStatus.Active,
            building.AdministrativeStatus);

        Assert.Equal(versionBeforeReactivation + 1, building.Version);
    }


    [Fact]
    public void ChangeAdministrativeStatus_ShouldNotChangeVersionWhenStatusIsTheSame()
    {
        // Arrange
        var building = CreateBuilding();

        var initialVersion = building.Version;

        // Act
        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Active);

        // Assert
        Assert.Equal(
            LocationAdministrativeStatus.Active,
            building.AdministrativeStatus);

        Assert.Equal(initialVersion, building.Version);
    }


    // ============================================================
    // ADD ZONE
    // ============================================================

    [Fact]
    public void AddZone_ShouldAddActiveZoneToActiveBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        var initialVersion = building.Version;

        // Act
        var zone = building.AddZone(
            "ZON-SERVER-01",
            "Sala de Servidores",
            "Data center principal",
            "Sótano 1");

        // Assert
        Assert.NotEqual(Guid.Empty, zone.Id);
        Assert.Equal(building.Id, zone.BuildingId);
        Assert.Equal("ZON-SERVER-01", zone.ZoneCode.Value);
        Assert.Equal("Sala de Servidores", zone.Name);
        Assert.Equal("Data center principal", zone.Description);
        Assert.Equal("Sótano 1", zone.FloorLabel);
        Assert.Equal(
            LocationAdministrativeStatus.Active,
            zone.AdministrativeStatus);

        Assert.Single(building.Zones);
        Assert.Contains(zone, building.Zones);

        Assert.Equal(initialVersion + 1, building.Version);
    }


    [Fact]
    public void AddZone_ShouldRejectInactiveBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Inactive);

        // Act
        var action = () => building.AddZone(
            "ZON-SERVER-01",
            "Sala de Servidores",
            "Data center principal",
            "Sótano 1");

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Cannot add a zone to an inactive building.",
            exception.Message);
    }


    [Fact]
    public void AddZone_ShouldRejectDuplicatedZoneCode()
    {
        // Arrange
        var building = CreateBuilding();

        building.AddZone(
            "ZON-SERVER-01",
            "Sala de Servidores",
            "Data center principal",
            "Sótano 1");

        // Act
        var action = () => building.AddZone(
            "ZON-SERVER-01",
            "Otra Sala",
            "Otra descripción",
            "Piso 1");

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "A zone with code 'ZON-SERVER-01' already exists in this building.",
            exception.Message);
    }


    [Fact]
    public void AddZone_ShouldAllowSameZoneCodeInDifferentBuildings()
    {
        // Arrange
        var firstBuilding = CreateBuilding();
        var secondBuilding = Building.Register(
            OrganizationId,
            BuildingCode.Create("BLD-SECOND-01"),
            "Segundo Edificio",
            null,
            CreateAddress());

        // Act
        var firstZone = firstBuilding.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        var secondZone = secondBuilding.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        // Assert
        Assert.Equal("ZON-01", firstZone.ZoneCode.Value);
        Assert.Equal("ZON-01", secondZone.ZoneCode.Value);
        Assert.NotEqual(firstBuilding.Id, secondBuilding.Id);
    }


    // ============================================================
    // UPDATE ZONE
    // ============================================================

    [Fact]
    public void UpdateZone_ShouldUpdateExistingZone()
    {
        // Arrange
        var building = CreateBuilding();

        var zone = building.AddZone(
            "ZON-01",
            "Sala 1",
            "Sala original",
            "Piso 1");

        var versionBeforeUpdate = building.Version;

        // Act
        building.UpdateZone(
            zone.Id,
            "Sala Actualizada",
            "Nueva descripción",
            "Piso 2");

        // Assert
        Assert.Equal("Sala Actualizada", zone.Name);
        Assert.Equal("Nueva descripción", zone.Description);
        Assert.Equal("Piso 2", zone.FloorLabel);
        Assert.Equal(versionBeforeUpdate + 1, building.Version);
    }


    [Fact]
    public void UpdateZone_ShouldRejectUnknownZone()
    {
        // Arrange
        var building = CreateBuilding();

        var unknownZoneId = Guid.NewGuid();

        // Act
        var action = () => building.UpdateZone(
            unknownZoneId,
            "Sala Actualizada",
            "Nueva descripción",
            "Piso 2");

        // Assert
        var exception = Assert.Throws<KeyNotFoundException>(action);

        Assert.Contains(
            unknownZoneId.ToString(),
            exception.Message);
    }


    // ============================================================
    // CHANGE ZONE ADMINISTRATIVE STATUS
    // ============================================================

    [Fact]
    public void ChangeZoneAdministrativeStatus_ShouldDeactivateZone()
    {
        // Arrange
        var building = CreateBuilding();

        var zone = building.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        var versionBeforeChange = building.Version;

        // Act
        building.ChangeZoneAdministrativeStatus(
            zone.Id,
            LocationAdministrativeStatus.Inactive);

        // Assert
        Assert.Equal(
            LocationAdministrativeStatus.Inactive,
            zone.AdministrativeStatus);

        Assert.Equal(versionBeforeChange + 1, building.Version);
    }


    [Fact]
    public void ChangeZoneAdministrativeStatus_ShouldReactivateZoneInActiveBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        var zone = building.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        building.ChangeZoneAdministrativeStatus(
            zone.Id,
            LocationAdministrativeStatus.Inactive);

        var versionBeforeReactivation = building.Version;

        // Act
        building.ChangeZoneAdministrativeStatus(
            zone.Id,
            LocationAdministrativeStatus.Active);

        // Assert
        Assert.Equal(
            LocationAdministrativeStatus.Active,
            zone.AdministrativeStatus);

        Assert.Equal(
            versionBeforeReactivation + 1,
            building.Version);
    }


    [Fact]
    public void ChangeZoneAdministrativeStatus_ShouldRejectActivationWhenBuildingIsInactive()
    {
        // Arrange
        var building = CreateBuilding();

        var zone = building.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        building.ChangeZoneAdministrativeStatus(
            zone.Id,
            LocationAdministrativeStatus.Inactive);

        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Inactive);

        // Act
        var action = () => building.ChangeZoneAdministrativeStatus(
            zone.Id,
            LocationAdministrativeStatus.Active);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Cannot activate a zone in an inactive building.",
            exception.Message);
    }


    [Fact]
    public void ChangeZoneAdministrativeStatus_ShouldRejectUnknownZone()
    {
        // Arrange
        var building = CreateBuilding();

        var unknownZoneId = Guid.NewGuid();

        // Act
        var action = () => building.ChangeZoneAdministrativeStatus(
            unknownZoneId,
            LocationAdministrativeStatus.Inactive);

        // Assert
        var exception = Assert.Throws<KeyNotFoundException>(action);

        Assert.Contains(
            unknownZoneId.ToString(),
            exception.Message);
    }


    // ============================================================
    // AVAILABILITY FOR DEVICE ASSIGNMENT
    // ============================================================

    [Fact]
    public void IsAvailableForAssignment_ShouldReturnTrueForActiveBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        // Act
        var result = building.IsAvailableForAssignment();

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void IsAvailableForAssignment_ShouldReturnFalseForInactiveBuilding()
    {
        // Arrange
        var building = CreateBuilding();

        building.ChangeAdministrativeStatus(
            LocationAdministrativeStatus.Inactive);

        // Act
        var result = building.IsAvailableForAssignment();

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsAvailableForAssignment_ShouldReturnTrueForActiveBuildingAndActiveZone()
    {
        // Arrange
        var building = CreateBuilding();

        var zone = building.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        // Act
        var result = building.IsAvailableForAssignment(zone.Id);

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void IsAvailableForAssignment_ShouldReturnFalseForInactiveZone()
    {
        // Arrange
        var building = CreateBuilding();

        var zone = building.AddZone(
            "ZON-01",
            "Sala 1",
            null,
            "Piso 1");

        building.ChangeZoneAdministrativeStatus(
            zone.Id,
            LocationAdministrativeStatus.Inactive);

        // Act
        var result = building.IsAvailableForAssignment(zone.Id);

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsAvailableForAssignment_ShouldReturnFalseForUnknownZone()
    {
        // Arrange
        var building = CreateBuilding();

        // Act
        var result = building.IsAvailableForAssignment(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}