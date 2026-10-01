using System.Text.RegularExpressions;

namespace ResQ.API.Device_Management.Domain.Model.ValueObjects;

public record DeviceCode
{
    private static readonly Regex ValidPattern =
        new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private DeviceCode(string value)
    {
        Value = value;
    }

    public static DeviceCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Device code is required.", nameof(value));

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length is < 1 or > 64)
            throw new ArgumentException(
                "Device code must contain between 1 and 64 characters.",
                nameof(value));

        if (!ValidPattern.IsMatch(normalized))
            throw new ArgumentException(
                "Device code can only contain ASCII letters, numbers, hyphens and underscores.",
                nameof(value));

        return new DeviceCode(normalized);
    }

    public override string ToString() => Value;

}