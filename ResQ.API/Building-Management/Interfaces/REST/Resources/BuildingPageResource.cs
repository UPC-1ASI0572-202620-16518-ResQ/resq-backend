namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record BuildingPageResource(
    List<BuildingResource> Items,
    int Page,
    int Size,
    long TotalElements,
    int TotalPages);
