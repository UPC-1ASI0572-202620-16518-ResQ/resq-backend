using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Domain.Model.Queries;

namespace ResQ.API.Connectivity_Management.Domain.Services;

public interface IConnectivityQueryService
{
    Task<SensorConnection?> Handle(GetSensorConnectionStatusQuery query);

    Task<IEnumerable<SensorConnection>> Handle(GetAllOfflineSensorsQuery query);
}
