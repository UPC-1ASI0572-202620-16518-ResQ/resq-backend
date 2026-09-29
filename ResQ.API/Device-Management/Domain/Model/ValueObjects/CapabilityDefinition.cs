namespace ResQ.API.Device_Management.Domain.Model.ValueObjects;

public record CapabilityDefinition
{
    public string Code { get; }
    public ECapabilityKind Kind { get; }
    public string? Unit { get; }

    private CapabilityDefinition()
    {
        Code = null!;
    }

    public CapabilityDefinition(
        string code,
        ECapabilityKind kind,
        string? unit)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Capability code is required.",
                nameof(code));

        var normalizedCode = code.Trim();

        if (normalizedCode.Length > 80)
            throw new ArgumentException(
                "Capability code cannot exceed 80 characters.",
                nameof(code));

        var normalizedUnit =
            string.IsNullOrWhiteSpace(unit)
                ? null
                : unit.Trim();

        if (normalizedUnit?.Length > 30)
            throw new ArgumentException(
                "Capability unit cannot exceed 30 characters.",
                nameof(unit));

        if (kind == ECapabilityKind.Actuation && normalizedUnit is not null)
            throw new ArgumentException(
                "Actuation capabilities cannot define a measurement unit.",
                nameof(unit));

        Code = normalizedCode;
        Kind = kind;
        Unit = normalizedUnit;
    }
}