using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Model.Commands;
using ResQ.API.Monitoring.Domain.Services;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.API.Monitoring.Infrastructure.ThingSpeak;

/// <summary>
/// Opt-in background importer. Reuses the existing monitoring command to
/// preserve downstream risk evaluation. Requires mapped, active device.
/// </summary>
public sealed class ThingSpeakImportWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ThingSpeakOptions> options,
    ILogger<ThingSpeakImportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("ThingSpeak import is disabled.");
            return;
        }

        if (settings.OrganizationId == Guid.Empty || settings.DeviceId == Guid.Empty)
        {
            logger.LogError("ThingSpeak import requires OrganizationId and DeviceId; import remains inactive.");
            return;
        }

        // The first poll establishes a baseline unless historical import was requested.
        // Duplicates on restart are checked against persisted Monitoring measurements.
        var initialized = settings.ImportHistorical;
        long baselineEntryId = 0;
        var period = TimeSpan.FromSeconds(Math.Clamp(settings.PollIntervalSeconds, 20, 3600));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<ThingSpeakFeedClient>();
                var rows = (await client.GetRecentEntriesAsync(stoppingToken))
                    .OrderBy(entry => entry.EntryId).ToList();

                if (!initialized)
                {
                    initialized = true;
                    baselineEntryId = rows.Count == 0 ? 0 : rows.Max(entry => entry.EntryId);
                    logger.LogInformation(
                        "ThingSpeak baseline established. Existing {Count} feed entries were not imported.", rows.Count);
                }
                else
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var monitoring = scope.ServiceProvider.GetRequiredService<IMonitoringCommandService>();
                    foreach (var row in rows)
                    {
                        if (row.EntryId <= baselineEntryId) continue;
                        foreach (var measurement in ThingSpeakMeasurementMapper.Map(row))
                        {
                            // Avoid duplicates after service restarts. Single-worker deployment expected.
                            var exists = await db.Set<Measurement>().AsNoTracking().AnyAsync(
                                m => m.OrganizationId == settings.OrganizationId &&
                                     m.DeviceId == settings.DeviceId &&
                                     m.VariableType == measurement.VariableType &&
                                     m.MeasuredAt == measurement.MeasuredAt,
                                stoppingToken);
                            if (exists) continue;

                            try
                            {
                                await monitoring.Handle(new RecordMeasurementCommand(
                                    settings.OrganizationId,
                                    settings.DeviceId,
                                    measurement.VariableType,
                                    measurement.Value,
                                    measurement.Unit,
                                    measurement.MeasuredAt));
                            }
                            catch (Exception error) when (!stoppingToken.IsCancellationRequested)
                            {
                                logger.LogWarning(error,
                                    "ThingSpeak ingestion failed for entry {EntryId}, field {Field}; will retry.",
                                    measurement.EntryId, measurement.FieldNumber);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception error)
            {
                logger.LogWarning(error, "ThingSpeak polling failed; will retry.");
            }

            try { await Task.Delay(period, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
