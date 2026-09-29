namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record AssignDeviceToLocationResource(Guid BuildingId, Guid? ZoneId);