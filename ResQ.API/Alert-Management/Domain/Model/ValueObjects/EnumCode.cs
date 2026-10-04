using System.Text;

namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Converts the context enums to and from the UPPER_SNAKE_CASE codes used by the
/// Alert and Response contract (e.g. HUMAN_REQUIRED, PENDING_AUTHORIZATION).
/// </summary>
public static class EnumCode
{
    /// <summary>
    /// Returns the UPPER_SNAKE_CASE code of an enum value.
    /// </summary>
    public static string ToCode<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        var builder = new StringBuilder(name.Length + 4);

        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i])) builder.Append('_');
            builder.Append(char.ToUpperInvariant(name[i]));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Parses a code written as UPPER_SNAKE_CASE, PascalCase or camelCase.
    /// </summary>
    /// <exception cref="ArgumentException">When the code does not match any value.</exception>
    public static TEnum Parse<TEnum>(string? code, string fieldName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException($"{fieldName} is required.");

        var normalized = code.Trim().Replace("_", string.Empty).Replace("-", string.Empty);

        if (int.TryParse(normalized, out _) || !Enum.TryParse<TEnum>(normalized, true, out var value) || !Enum.IsDefined(value))
        {
            var allowed = string.Join(", ", Enum.GetValues<TEnum>().Select(ToCode));
            throw new ArgumentException($"Invalid {fieldName} '{code}'. Allowed values: {allowed}.");
        }

        return value;
    }
}
