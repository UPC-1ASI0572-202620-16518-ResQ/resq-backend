using System.Text.RegularExpressions;

namespace ResQ.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
/// Normalization helpers for Monitoring contract codes.
/// </summary>
public static partial class MonitoringCodes
{
    public static string NormalizeVariableType(string? variableType)
    {
        if (string.IsNullOrWhiteSpace(variableType))
            throw new ArgumentException("Variable type is required.");

        var normalized = variableType.Trim().ToUpperInvariant().Replace('-', '_').Replace(' ', '_');

        if (normalized.Length > 50 || !CodePattern().IsMatch(normalized))
            throw new ArgumentException("Variable type must use letters, digits and underscores (e.g. GAS_PPM), up to 50 characters.");

        return normalized;
    }

    public static string ToCode(EDeviceAvailability availability)
    {
        return availability switch
        {
            EDeviceAvailability.Available => "AVAILABLE",
            EDeviceAvailability.Unavailable => "UNAVAILABLE",
            _ => "UNKNOWN"
        };
    }

    [GeneratedRegex("^[A-Z0-9_]+$")]
    private static partial Regex CodePattern();
}