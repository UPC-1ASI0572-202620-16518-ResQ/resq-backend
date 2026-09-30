using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record ChangeZoneAdministrativeStatusCommand(
    Guid OrganizationId,
    Guid BuildingId,
    Guid ZoneId,
    LocationAdministrativeStatus AdministrativeStatus);
