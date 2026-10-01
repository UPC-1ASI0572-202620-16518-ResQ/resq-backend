using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Commands;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Building_Management.Domain.Repositories;
using ResQ.API.Building_Management.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Building_Management.Application.Internal.CommandServices;

public class BuildingCommandService(IBuildingRepository buildingRepository, IUnitOfWork unitOfWork)
    : IBuildingCommandService
{
    public async Task<Building> Handle(RegisterBuildingCommand command)
    {
        var buildingCode = BuildingCode.Create(command.BuildingCode);

        var codeExists = await buildingRepository
            .ExistsByOrganizationIdAndBuildingCodeAsync(command.OrganizationId, buildingCode);

        if (codeExists)
            throw new InvalidOperationException(
                "A building with this code already exists in the organization.");

        var building = Building.Register(
            command.OrganizationId,
            buildingCode,
            command.Name,
            command.Description,
            command.Address);

        await buildingRepository.AddAsync(building);
        await unitOfWork.CompleteAsync();

        return building;
    }

    public async Task<Building> Handle(UpdateBuildingDetailsCommand command)
    {
        var building = await FindBuildingOrThrow(command.OrganizationId, command.BuildingId);

        var newName = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Name)
            ? building.Name
            : command.Name!.Trim();

        var newDescription = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Description)
            ? building.Description
            : command.Description?.Trim();

        var existingAddress = building.Address;
        var newStreet = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.StreetAddress)
            ? existingAddress.StreetAddress
            : command.StreetAddress!.Trim();

        var newDistrict = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.District)
            ? existingAddress.District
            : command.District!.Trim();

        var newCity = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.City)
            ? existingAddress.City
            : command.City!.Trim();

        var newCountry = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.CountryCode)
            ? existingAddress.CountryCode
            : command.CountryCode!.Trim().ToUpperInvariant();

        var newAddress = new BuildingAddress(newStreet, newDistrict, newCity, newCountry);

        building.UpdateDetails(newName, newDescription, newAddress);

        buildingRepository.Update(building);
        await unitOfWork.CompleteAsync();

        return building;
    }

    public async Task<Building> Handle(ChangeBuildingAdministrativeStatusCommand command)
    {
        var building = await FindBuildingOrThrow(command.OrganizationId, command.BuildingId);

        building.ChangeAdministrativeStatus(command.AdministrativeStatus);

        buildingRepository.Update(building);
        await unitOfWork.CompleteAsync();

        return building;
    }

    public async Task<Building> Handle(AddZoneToBuildingCommand command)
    {
        var building = await FindBuildingOrThrow(command.OrganizationId, command.BuildingId);

        building.AddZone(command.ZoneCode, command.Name, command.Description, command.FloorLabel);

        buildingRepository.Update(building);
        await unitOfWork.CompleteAsync();

        return building;
    }

    public async Task<Building> Handle(UpdateZoneCommand command)
    {
        var building = await FindBuildingOrThrow(command.OrganizationId, command.BuildingId);

        var zone = building.Zones.FirstOrDefault(z => z.Id == command.ZoneId)
            ?? throw new KeyNotFoundException($"Zone '{command.ZoneId}' not found in this building.");

        var newName = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Name)
            ? zone.Name
            : command.Name!.Trim();

        var newDescription = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.Description)
            ? zone.Description
            : command.Description?.Trim();

        var newFloorLabel = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(command.FloorLabel)
            ? zone.FloorLabel
            : command.FloorLabel?.Trim();

        building.UpdateZone(command.ZoneId, newName, newDescription, newFloorLabel);

        buildingRepository.Update(building);
        await unitOfWork.CompleteAsync();

        return building;
    }

    public async Task<Building> Handle(ChangeZoneAdministrativeStatusCommand command)
    {
        var building = await FindBuildingOrThrow(command.OrganizationId, command.BuildingId);

        building.ChangeZoneAdministrativeStatus(command.ZoneId, command.AdministrativeStatus);

        buildingRepository.Update(building);
        await unitOfWork.CompleteAsync();

        return building;
    }

    private async Task<Building> FindBuildingOrThrow(Guid organizationId, Guid buildingId)
    {
        var building = await buildingRepository.FindByIdAndOrganizationIdAsync(buildingId, organizationId);
        if (building == null)
            throw new KeyNotFoundException("Building not found.");
        return building;
    }
}
