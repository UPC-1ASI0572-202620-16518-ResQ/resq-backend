namespace ResQ.API.Risk_Detection.Interfaces.REST.Transform;

internal static class DecimalFormat
{
    /// <summary>
    /// Removes the trailing zeros added by the decimal(18,4) columns (650.0000 → 650).
    /// </summary>
    public static decimal Normalize(decimal value) => value / 1.0000000000000000000000000000m;
}
