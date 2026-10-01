using System.Text.RegularExpressions;

namespace ResQ.API.Building_Management.Domain.Model.ValueObjects;

/// <summary>
/// Unique code for a zone within a building.
/// </summary>
public record ZoneCode
{
    private static readonly Regex ValidPattern =
        new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private ZoneCode(string value)
    {
        Value = value;
    }

    public static ZoneCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Zone code is required.", nameof(value));

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length is < 1 or > 64)
            throw new ArgumentException(
                "Zone code must contain between 1 and 64 characters.",
                nameof(value));

        if (!ValidPattern.IsMatch(normalized))
            throw new ArgumentException(
                "Zone code can only contain ASCII letters, numbers, hyphens and underscores.",
                nameof(value));

        return new ZoneCode(normalized);
    }

    public override string ToString() => Value;
}
