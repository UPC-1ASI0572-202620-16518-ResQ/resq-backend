using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Interfaces.REST.Resources;

public record DeviceCapabilityResource(Guid Id, string Code, ECapabilityKind Kind, string? Unit);