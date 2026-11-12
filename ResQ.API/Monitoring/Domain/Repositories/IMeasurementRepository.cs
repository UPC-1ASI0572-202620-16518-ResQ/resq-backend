using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Monitoring.Domain.Repositories;

public interface IMeasurementRepository : IBaseRepository<Measurement>
{
    Task<IEnumerable<Measurement>> FindCurrentByDeviceIdAsync(Guid organizationId, Guid deviceId, DateTimeOffset minimumMeasuredAt);

    Task<IEnumerable<Measurement>> FindCurrentByZoneIdAsync(Guid organizationId, Guid zoneId, DateTimeOffset minimumMeasuredAt);
}