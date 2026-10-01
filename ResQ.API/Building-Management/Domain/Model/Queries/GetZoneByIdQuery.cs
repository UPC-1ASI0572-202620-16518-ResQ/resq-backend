namespace ResQ.API.Building_Management.Domain.Model.Queries;

public record GetZoneByIdQuery(Guid OrganizationId, Guid BuildingId, Guid ZoneId);
