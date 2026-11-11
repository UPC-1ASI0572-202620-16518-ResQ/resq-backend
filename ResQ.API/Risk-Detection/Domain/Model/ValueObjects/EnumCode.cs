using System.Text;

namespace ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

/// <summary>
/// Converts the context enums to and from the UPPER_SNAKE_CASE codes used by the
/// Risk Detection contract (e.g. GREATER_THAN_OR_EQUAL, UNRESOLVED).
/// </summary>
public static class EnumCode
{
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
