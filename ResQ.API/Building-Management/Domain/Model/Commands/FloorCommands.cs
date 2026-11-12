namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record AddFloorCommand(Guid OrganizationId, Guid BuildingId, string Label, int Level);
public record UpdateFloorCommand(Guid OrganizationId, Guid BuildingId, Guid FloorId, string Label, int Level);
public record RemoveFloorCommand(Guid OrganizationId, Guid BuildingId, Guid FloorId);
