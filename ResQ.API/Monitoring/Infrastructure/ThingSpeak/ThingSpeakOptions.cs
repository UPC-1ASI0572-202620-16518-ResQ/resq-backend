namespace ResQ.API.Monitoring.Infrastructure.ThingSpeak;

/// <summary>
/// Server-only configuration. On Render, use ThingSpeak__ReadApiKey as a secret env var.
/// </summary>
public sealed class ThingSpeakOptions
{
    public const string SectionName = "ThingSpeak";

    public long ChannelId { get; set; } = 3577021;

    public string ReadApiKey { get; set; } = string.Empty;

    public int RequestTimeoutSeconds { get; set; } = 15;

    public int FetchCount { get; set; } = 10;

    public bool Enabled { get; set; } = false;

    public Guid OrganizationId { get; set; } = Guid.Empty;

    public Guid DeviceId { get; set; } = Guid.Empty;

    public int PollIntervalSeconds { get; set; } = 30;

    public bool ImportHistorical { get; set; } = false;

    // Explicit, independent opt-in for public prototype measurements.
    public bool PublicDemoEnabled { get; set; } = false;
}
