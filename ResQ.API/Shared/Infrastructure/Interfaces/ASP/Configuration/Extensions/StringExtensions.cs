using System.Text.RegularExpressions;

namespace ResQ.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
///     Provides extension methods for string manipulation related to ASP.NET configuration.
/// </summary>
public static partial class StringExtensions
{
    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", RegexOptions.Compiled)]
    private static partial Regex KebabCaseRegex();

    /// <summary>
    ///     Converts a string to kebab-case casing, e.g. "MyString" becomes "my-string"
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The string in the kebab-case.</returns>
    public static string ToKebabCase(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return KebabCaseRegex().Replace(text, "-$1")
            .Trim()
            .ToLower();
    }
}