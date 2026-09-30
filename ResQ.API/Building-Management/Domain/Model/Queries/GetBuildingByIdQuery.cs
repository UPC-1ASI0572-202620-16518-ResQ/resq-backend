namespace ResQ.API.Building_Management.Domain.Model.Queries;

public record GetBuildingByIdQuery(Guid OrganizationId, Guid BuildingId);
