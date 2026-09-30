using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record ChangeBuildingAdministrativeStatusCommand(
    Guid OrganizationId,
    Guid BuildingId,
    LocationAdministrativeStatus AdministrativeStatus);
