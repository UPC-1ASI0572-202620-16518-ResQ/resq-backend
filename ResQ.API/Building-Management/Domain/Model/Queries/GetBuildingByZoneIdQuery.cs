namespace ResQ.API.Building_Management.Domain.Model.Queries;

public record GetBuildingByZoneIdQuery(Guid OrganizationId, Guid ZoneId);
