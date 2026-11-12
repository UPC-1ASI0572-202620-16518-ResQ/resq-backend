namespace ResQ.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
/// Defines the maximum age for a measurement to be considered current.
/// </summary>
public record FreshnessPolicy
{
    public TimeSpan MaximumAge { get; }

    public FreshnessPolicy(TimeSpan maximumAge)
    {
        if (maximumAge <= TimeSpan.Zero)
            throw new ArgumentException("Maximum measurement age must be greater than zero.", nameof(maximumAge));

        MaximumAge = maximumAge;
    }

    public bool IsFresh(DateTimeOffset measuredAt, DateTimeOffset referenceTime)
    {
        if (measuredAt > referenceTime)
            return false;

        return referenceTime - measuredAt <= MaximumAge;
    }
}