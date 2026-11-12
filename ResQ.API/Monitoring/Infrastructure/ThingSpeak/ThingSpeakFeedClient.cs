using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ResQ.API.Monitoring.Infrastructure.ThingSpeak;

/// <summary>
/// A read-only ThingSpeak client; credentials remain server-side.
/// No measurements are written until an idempotent import service is enabled.
/// </summary>
public sealed class ThingSpeakFeedClient(
    HttpClient httpClient,
    IOptions<ThingSpeakOptions> options)
{
    public async Task<IReadOnlyList<ThingSpeakFeedEntry>> GetRecentEntriesAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (settings.ChannelId <= 0)
            throw new InvalidOperationException("ThingSpeak channel ID must be positive.");
        if (string.IsNullOrWhiteSpace(settings.ReadApiKey))
            throw new InvalidOperationException("ThingSpeak ReadApiKey is not configured on the server.");

        var count = Math.Clamp(settings.FetchCount, 1, 100);
        var path = $"/channels/{settings.ChannelId}/feeds.json?api_key={Uri.EscapeDataString(settings.ReadApiKey)}&results={count}";
        // Avoid logging the URI: its query string contains the private read key.
        using var response = await httpClient.GetAsync(
            path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var payload = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!payload.RootElement.TryGetProperty("feeds", out var feeds) ||
            feeds.ValueKind != JsonValueKind.Array)
            throw new JsonException("ThingSpeak response did not contain a feeds array.");

        var entries = new List<ThingSpeakFeedEntry>();
        foreach (var feed in feeds.EnumerateArray())
        {
            if (!feed.TryGetProperty("entry_id", out var id) ||
                !id.TryGetInt64(out var entryId) ||
                !feed.TryGetProperty("created_at", out var timestamp) ||
                timestamp.ValueKind != JsonValueKind.String ||
                !DateTimeOffset.TryParse(timestamp.GetString(), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var createdAt))
                continue;

            var values = new decimal?[6];
            for (var index = 1; index <= 6; index++)
            {
                if (feed.TryGetProperty($"field{index}", out var field) &&
                    field.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(field.GetString(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var value))
                    values[index - 1] = value;
            }

            entries.Add(new ThingSpeakFeedEntry(entryId, createdAt.ToUniversalTime(), values));
        }

        return entries;
    }
}

public sealed record ThingSpeakFeedEntry(
    long EntryId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<decimal?> Fields);
