namespace ResQ.API.Shared.Application.Internal;

public static class PartialUpdateHelper
{
    /// <summary>
    /// Determines whether an input value should be ignored during updates
    /// (e.g. if it is null, whitespace, or Swagger's default placeholder "string").
    /// </summary>
    public static bool ShouldIgnore(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || value.Trim().Equals("string", StringComparison.OrdinalIgnoreCase);
    }
}
