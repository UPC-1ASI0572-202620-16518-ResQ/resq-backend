using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Commands;

namespace ResQ.API.Building_Management.Domain.Services;

public interface IBuildingCommandService
{
    Task<Building> Handle(RegisterBuildingCommand command);
    Task<Building> Handle(UpdateBuildingDetailsCommand command);
    Task<Building> Handle(ChangeBuildingAdministrativeStatusCommand command);
    Task<Building> Handle(AddZoneToBuildingCommand command);
    Task<Building> Handle(UpdateZoneCommand command);
    Task<Building> Handle(ChangeZoneAdministrativeStatusCommand command);
}
