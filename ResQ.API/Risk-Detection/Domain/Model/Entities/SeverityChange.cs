namespace ResQ.API.Risk_Detection.Domain.Model.Entities;

/// <summary>
/// Preserves a transition between severities of a detection. Belongs to the RiskDetection aggregate.
/// </summary>
public class SeverityChange
{
    public Guid Id { get; private set; }

    public string PreviousSeverityCode { get; private set; } = string.Empty;

    public string NewSeverityCode { get; private set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected SeverityChange()
    {
    }

    internal SeverityChange(string previousSeverityCode, string newSeverityCode, DateTimeOffset changedAt)
    {
        Id = Guid.NewGuid();
        PreviousSeverityCode = previousSeverityCode;
        NewSeverityCode = newSeverityCode;
        ChangedAt = changedAt;
    }
}
