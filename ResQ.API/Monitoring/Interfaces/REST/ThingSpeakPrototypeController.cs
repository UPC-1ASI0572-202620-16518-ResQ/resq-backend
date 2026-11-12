using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Monitoring.Infrastructure.ThingSpeak;

namespace ResQ.API.Monitoring.Interfaces.REST;

/// <summary>
/// Opt-in, read-only, public demo of six telemetry values.
/// This bypasses the business device registry by design and never returns API keys.
/// Do not enable for sensitive deployments.
/// </summary>
[ApiController]
[Route("api/v1/prototype/telemetry")]
[AllowAnonymous]
[Produces("application/json")]
public sealed class ThingSpeakPrototypeController(
    ThingSpeakFeedClient client,
    IOptions<ThingSpeakOptions> options,
    IMemoryCache cache,
    ILogger<ThingSpeakPrototypeController> logger) : ControllerBase
{
    [HttpGet("latest")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> Latest(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.PublicDemoEnabled)
            return NotFound(new { message = "Prototype telemetry is not enabled." });

        if (string.IsNullOrWhiteSpace(settings.ReadApiKey))
            return StatusCode(503, new { message = "Prototype telemetry is not configured." });

        try
        {
            // Shared application cache protects ThingSpeak from aggressive browser refreshes.
            var snapshot = await cache.GetOrCreateAsync(
                $"thingspeak-prototype-{settings.ChannelId}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(25);
                    var feeds = await client.GetRecentEntriesAsync(cancellationToken);
                    var last = feeds.OrderByDescending(f => f.EntryId).FirstOrDefault();
                    if (last is null)
                        return null;

                    var v = last.Fields;
                    return new PrototypeTelemetryResource(
                        settings.ChannelId,
                        last.EntryId,
                        last.CreatedAt,
                        v.ElementAtOrDefault(0),
                        v.ElementAtOrDefault(1),
                        v.ElementAtOrDefault(2),
                        v.ElementAtOrDefault(3),
                        v.ElementAtOrDefault(4),
                        v.ElementAtOrDefault(5));
                });

            if (snapshot is null)
                return Ok(new { channelId = settings.ChannelId, message = "No telemetry samples yet." });

            return Ok(snapshot);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning("ThingSpeak prototype query failed with status {StatusCode}.", exception.StatusCode);
            return StatusCode(502, new { message = "Telemetry provider temporarily unavailable." });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception.GetType().Name, "ThingSpeak prototype response could not be processed.");
            return StatusCode(502, new { message = "Telemetry could not be read." });
        }
    }
}

public sealed record PrototypeTelemetryResource(
    long ChannelId,
    long EntryId,
    DateTimeOffset MeasuredAt,
    decimal? GasAdc,
    decimal? TemperatureC,
    decimal? WifiRssiDbm,
    decimal? AlertFlag,
    decimal? Motion,
    decimal? HumidityPercent);
