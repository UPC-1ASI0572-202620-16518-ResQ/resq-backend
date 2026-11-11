using Microsoft.Extensions.Options;
using ResQ.API.Connectivity_Management.Domain.Model.Commands;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Services;

namespace ResQ.API.Connectivity_Management.Application.Internal.EventHandlers;

/// <summary>
/// Periodically evaluates sensor timeouts (EvaluateSensorTimeoutsCommand): every ONLINE sensor whose last heartbeat
/// is older than ConnectivitySettings:HeartbeatTimeoutSeconds is marked TIMEOUT and a disconnection alert is raised.
/// </summary>
public class SensorTimeoutBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<ConnectivitySettings> settings,
    ILogger<SensorTimeoutBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(settings.Value.TimeoutScanInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var commandService = scope.ServiceProvider.GetRequiredService<IConnectivityCommandService>();

                var timedOut = await commandService.Handle(new EvaluateSensorTimeoutsCommand(DateTimeOffset.UtcNow));

                if (timedOut.Count > 0)
                    logger.LogInformation("{Count} sensor(s) marked as TIMEOUT.", timedOut.Count);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                // A database outage must not stop the evaluation loop
                logger.LogError(e, "Error evaluating sensor timeouts.");
            }
        }
    }
}
