namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record DeviceAssignmentResource(Guid BuildingId, Guid? ZoneId);