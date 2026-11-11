using Microsoft.Extensions.Options;
using ResQ.API.Connectivity_Management.Domain.Model.Commands;
using ResQ.API.Connectivity_Management.Domain.Model.Queries;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Services;

namespace ResQ.API.Connectivity_Management.Interfaces.ACL;

/// <summary>
/// Implementation of the Anti-Corruption Layer facade for Connectivity Management.
/// </summary>
public class ConnectivityContextFacade(
    IConnectivityCommandService connectivityCommandService,
    IConnectivityQueryService connectivityQueryService,
    IOptions<ConnectivitySettings> settings) : IConnectivityContextFacade
{
    public async Task<string?> GetSensorConnectionStatusAsync(Guid organizationId, Guid sensorId)
    {
        var connection = await connectivityQueryService.Handle(new GetSensorConnectionStatusQuery(organizationId, sensorId));

        return connection?
            .GetEffectiveStatus(DateTimeOffset.UtcNow, settings.Value.HeartbeatTimeout)
            .ToString()
            .ToUpperInvariant();
    }

    public async Task<bool> MarkSensorOfflineAsync(Guid organizationId, Guid sensorId)
    {
        var connection = await connectivityCommandService.Handle(new MarkSensorOfflineCommand(organizationId, sensorId));
        return connection is not null;
    }
}
