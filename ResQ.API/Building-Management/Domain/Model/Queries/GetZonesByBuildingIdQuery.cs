using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Queries;

public record GetZonesByBuildingIdQuery(
    Guid OrganizationId,
    Guid BuildingId,
    LocationAdministrativeStatus? AdministrativeStatus,
    int Page,
    int Size);
