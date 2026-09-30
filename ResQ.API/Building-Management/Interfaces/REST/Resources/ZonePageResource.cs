namespace ResQ.API.Building_Management.Interfaces.REST.Resources;

public record ZonePageResource(
    List<ZoneResource> Items,
    int Page,
    int Size,
    long TotalElements,
    int TotalPages);
