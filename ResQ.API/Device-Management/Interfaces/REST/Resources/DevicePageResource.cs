namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record DevicePageResource(List<DeviceResource> Items, int Page, int Size,
    long TotalElements, int TotalPages);