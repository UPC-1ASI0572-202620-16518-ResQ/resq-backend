using System.Text.RegularExpressions;

namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Normalization rules for the risk type and severity codes received from Risk Detection.
/// </summary>
public static partial class RiskCodes
{
    /// <summary>
    /// Severity codes known by the platform.
    /// </summary>
    public static readonly IReadOnlyList<string> Severities = ["Info", "Warning", "Critical"];

    /// <summary>
    /// Risk type codes are UPPER_SNAKE_CASE identifiers such as GAS_LEAK or FIRE.
    /// </summary>
    public static string NormalizeRiskTypeCode(string? riskTypeCode)
    {
        if (string.IsNullOrWhiteSpace(riskTypeCode))
            throw new ArgumentException("Risk type code is required.");

        var normalized = riskTypeCode.Trim().ToUpperInvariant();

        if (normalized.Length > 50 || !RiskTypeCodePattern().IsMatch(normalized))
            throw new ArgumentException("Risk type code must use letters, digits and underscores (e.g. GAS_LEAK), up to 50 characters.");

        return normalized;
    }

    /// <summary>
    /// Severity codes are Info, Warning or Critical.
    /// </summary>
    public static string NormalizeSeverityCode(string? severityCode)
    {
        if (string.IsNullOrWhiteSpace(severityCode))
            throw new ArgumentException("Severity code is required.");

        var match = Severities.FirstOrDefault(severity =>
            string.Equals(severity, severityCode.Trim(), StringComparison.OrdinalIgnoreCase));

        return match ?? throw new ArgumentException(
            $"Invalid severity code '{severityCode}'. Allowed values: {string.Join(", ", Severities)}.");
    }

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$")]
    private static partial Regex RiskTypeCodePattern();
}
