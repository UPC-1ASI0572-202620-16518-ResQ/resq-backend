using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Monitoring.Domain.Repositories;

public interface IMeasurementRepository : IBaseRepository<Measurement>
{
    Task<IEnumerable<Measurement>> FindCurrentByDeviceIdAsync(Guid organizationId, Guid deviceId, DateTimeOffset minimumMeasuredAt);

    Task<IEnumerable<Measurement>> FindCurrentByZoneIdAsync(Guid organizationId, Guid zoneId, DateTimeOffset minimumMeasuredAt);

    Task<IEnumerable<Measurement>> FindHistoryByDeviceIdAsync(
        Guid organizationId, Guid deviceId, DateTimeOffset? from, DateTimeOffset? to, int limit);

    Task<IEnumerable<Measurement>> FindHistoryByZoneIdAsync(
        Guid organizationId, Guid zoneId, DateTimeOffset? from, DateTimeOffset? to, int limit);
}
