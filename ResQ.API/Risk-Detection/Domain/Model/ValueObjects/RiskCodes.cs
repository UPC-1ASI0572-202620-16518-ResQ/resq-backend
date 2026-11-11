using System.Text.RegularExpressions;

namespace ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

/// <summary>
/// Normalization of the RiskType, SeverityLevel and variable type codes.
/// The severity catalog (Info, Warning, Critical) is the one accepted by Alert &amp; Response,
/// so every detection can continue the emergency flow as an alert.
/// </summary>
public static partial class RiskCodes
{
    /// <summary>
    /// Severity codes ordered from the least to the most severe.
    /// </summary>
    public static readonly IReadOnlyList<string> Severities = ["Info", "Warning", "Critical"];

    /// <summary>
    /// Risk type codes are UPPER_SNAKE_CASE identifiers such as GAS_LEAK or FIRE (no closed catalog).
    /// </summary>
    public static string NormalizeRiskTypeCode(string? riskTypeCode)
    {
        if (string.IsNullOrWhiteSpace(riskTypeCode))
            throw new ArgumentException("Risk type code is required.");

        var normalized = riskTypeCode.Trim().ToUpperInvariant();

        if (normalized.Length > 50 || !CodePattern().IsMatch(normalized))
            throw new ArgumentException("Risk type code must use letters, digits and underscores (e.g. GAS_LEAK), up to 50 characters.");

        return normalized;
    }

    public static string NormalizeSeverityCode(string? severityCode)
    {
        if (string.IsNullOrWhiteSpace(severityCode))
            throw new ArgumentException("Severity code is required.");

        var match = Severities.FirstOrDefault(severity =>
            string.Equals(severity, severityCode.Trim(), StringComparison.OrdinalIgnoreCase));

        return match ?? throw new ArgumentException(
            $"Invalid severity code '{severityCode}'. Allowed values: {string.Join(", ", Severities)}.");
    }

    /// <summary>
    /// Monitored variable types are UPPER_SNAKE_CASE identifiers such as GAS_PPM or TEMPERATURE.
    /// </summary>
    public static string NormalizeVariableType(string? variableType)
    {
        if (string.IsNullOrWhiteSpace(variableType))
            throw new ArgumentException("Variable type is required.");

        var normalized = variableType.Trim().ToUpperInvariant().Replace('-', '_').Replace(' ', '_');

        if (normalized.Length > 50 || !CodePattern().IsMatch(normalized))
            throw new ArgumentException("Variable type must use letters, digits and underscores (e.g. GAS_PPM), up to 50 characters.");

        return normalized;
    }

    /// <summary>
    /// Position of a severity in the catalog; a higher rank is more severe.
    /// </summary>
    public static int SeverityRank(string severityCode)
    {
        return Severities.ToList().FindIndex(severity =>
            string.Equals(severity, severityCode, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$")]
    private static partial Regex CodePattern();
}
