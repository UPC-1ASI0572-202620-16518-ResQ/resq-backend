namespace ResQ.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
/// Monitoring configuration bound from the MonitoringSettings section.
/// </summary>
public class MonitoringSettings
{
    public const string SectionName = "MonitoringSettings";

    /// <summary>
    /// Maximum age in seconds for a measurement to be considered current.
    /// </summary>
    public int MeasurementMaxAgeSeconds { get; set; } = 300;

    public FreshnessPolicy FreshnessPolicy =>
        new(TimeSpan.FromSeconds(Math.Max(1, MeasurementMaxAgeSeconds)));
}