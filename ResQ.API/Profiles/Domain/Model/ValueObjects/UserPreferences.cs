namespace ResQ.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value Object for specific user preferences in the platform.
/// </summary>
public record UserPreferences(string TimeZone, bool ReceiveSmsAlerts)
{
    public UserPreferences() : this("UTC", false)
    {
    }

    /// <summary>
    ///     Checks if SMS alerts are enabled.
    /// </summary>
    public bool IsSmsEnabled() => ReceiveSmsAlerts;
}
