using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Domain.Model.Commands;

public record RegisterBuildingCommand(
    Guid OrganizationId,
    string BuildingCode,
    string Name,
    string? Description,
    BuildingAddress Address,
    IReadOnlyCollection<FloorDefinition>? Floors = null);

public record FloorDefinition(string Label, int Level);
