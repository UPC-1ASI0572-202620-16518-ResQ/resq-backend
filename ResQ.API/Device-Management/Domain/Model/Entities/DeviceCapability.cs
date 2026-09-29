using ResQ.API.Device_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Device_Management.Domain.Model.Entities;

/// <summary>
/// Represents a measurement or actuation capability
/// that belongs to a Device.
/// </summary>
public class DeviceCapability
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public ECapabilityKind Kind { get; private set; }

    public string? Unit { get; private set; }

    protected DeviceCapability()
    {
    }

    public DeviceCapability(Guid id, CapabilityDefinition definition)
    {
        Id = id;
        Code = definition.Code;
        Kind = definition.Kind;
        Unit = definition.Unit;
    }

    internal void Update(CapabilityDefinition definition)
    {
        Code = definition.Code;
        Kind = definition.Kind;
        Unit = definition.Unit;
    }
}