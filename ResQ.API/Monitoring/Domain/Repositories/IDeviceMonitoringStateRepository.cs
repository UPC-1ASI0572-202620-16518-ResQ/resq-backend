using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Monitoring.Domain.Repositories;

public interface IDeviceMonitoringStateRepository : IBaseRepository<DeviceMonitoringState>
{
    Task<DeviceMonitoringState?> FindByDeviceIdAsync(Guid organizationId, Guid deviceId);

    Task<IEnumerable<DeviceMonitoringState>> FindAllAsync(Guid organizationId);
}