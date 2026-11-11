namespace ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

/// <summary>
/// Risk Detection configuration, bound from the "RiskDetectionSettings" section of appsettings.
/// </summary>
public class RiskDetectionSettings
{
    public const string SectionName = "RiskDetectionSettings";

    /// <summary>
    /// A new measurement of the same risk type and location within this window is added as evidence
    /// to the existing detection instead of creating a new one.
    /// </summary>
    public int CorrelationWindowMinutes { get; set; } = 30;

    /// <summary>
    /// When true, new detections and severity escalations generate an alert in Alert &amp; Response.
    /// </summary>
    public bool RaiseAlerts { get; set; } = true;

    public TimeSpan CorrelationWindow => TimeSpan.FromMinutes(Math.Max(0, CorrelationWindowMinutes));
}
