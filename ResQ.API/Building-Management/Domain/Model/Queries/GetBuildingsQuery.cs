using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Queries;

public record GetBuildingsQuery(
    Guid OrganizationId,
    LocationAdministrativeStatus? AdministrativeStatus,
    int Page,
    int Size);
