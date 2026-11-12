using Microsoft.EntityFrameworkCore;
using ResQ.API.Monitoring.Domain.Model.Aggregates;
using ResQ.API.Monitoring.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Monitoring.Infrastructure.Persistence.EFC.Repositories;

public class MeasurementRepository(AppDbContext context) : BaseRepository<Measurement>(context), IMeasurementRepository
{
    public async Task<IEnumerable<Measurement>> FindCurrentByDeviceIdAsync(
        Guid organizationId,
        Guid deviceId,
        DateTimeOffset minimumMeasuredAt)
    {
        return await Context.Set<Measurement>()
            .Where(measurement =>
                measurement.OrganizationId == organizationId &&
                measurement.DeviceId == deviceId &&
                measurement.MeasuredAt >= minimumMeasuredAt)
            .OrderByDescending(measurement => measurement.MeasuredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Measurement>> FindCurrentByZoneIdAsync(
        Guid organizationId,
        Guid zoneId,
        DateTimeOffset minimumMeasuredAt)
    {
        return await Context.Set<Measurement>()
            .Where(measurement =>
                measurement.OrganizationId == organizationId &&
                measurement.ZoneId == zoneId &&
                measurement.MeasuredAt >= minimumMeasuredAt)
            .OrderByDescending(measurement => measurement.MeasuredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Measurement>> FindHistoryByDeviceIdAsync(
        Guid organizationId, Guid deviceId, DateTimeOffset? from, DateTimeOffset? to, int limit)
    {
        var query = Context.Set<Measurement>().Where(measurement =>
            measurement.OrganizationId == organizationId && measurement.DeviceId == deviceId);
        if (from.HasValue) query = query.Where(measurement => measurement.MeasuredAt >= from.Value);
        if (to.HasValue) query = query.Where(measurement => measurement.MeasuredAt <= to.Value);
        return await query.OrderByDescending(measurement => measurement.MeasuredAt)
            .Take(Math.Clamp(limit, 1, 1000)).ToListAsync();
    }

    public async Task<IEnumerable<Measurement>> FindHistoryByZoneIdAsync(
        Guid organizationId, Guid zoneId, DateTimeOffset? from, DateTimeOffset? to, int limit)
    {
        var query = Context.Set<Measurement>().Where(measurement =>
            measurement.OrganizationId == organizationId && measurement.ZoneId == zoneId);
        if (from.HasValue) query = query.Where(measurement => measurement.MeasuredAt >= from.Value);
        if (to.HasValue) query = query.Where(measurement => measurement.MeasuredAt <= to.Value);
        return await query.OrderByDescending(measurement => measurement.MeasuredAt)
            .Take(Math.Clamp(limit, 1, 1000)).ToListAsync();
    }
}
