using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record ChangeZoneAdministrativeStatusResource(
    LocationAdministrativeStatus AdministrativeStatus);
