using Microsoft.Extensions.Options;
using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Domain.Model.Queries;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Repositories;
using ResQ.API.Connectivity_Management.Domain.Services;

namespace ResQ.API.Connectivity_Management.Application.Internal.QueryServices;

public class ConnectivityQueryService(
    ISensorConnectionRepository sensorConnectionRepository,
    IOptions<ConnectivitySettings> settings) : IConnectivityQueryService
{
    public async Task<SensorConnection?> Handle(GetSensorConnectionStatusQuery query)
    {
        return await sensorConnectionRepository.FindBySensorIdAsync(query.OrganizationId, query.SensorId);
    }

    public async Task<IEnumerable<SensorConnection>> Handle(GetAllOfflineSensorsQuery query)
    {
        // Sensors whose heartbeat expired are included even if the background evaluation has not marked them yet
        var heartbeatLimit = DateTimeOffset.UtcNow - settings.Value.HeartbeatTimeout;
        return await sensorConnectionRepository.FindDisconnectedAsync(query.OrganizationId, heartbeatLimit);
    }
}
