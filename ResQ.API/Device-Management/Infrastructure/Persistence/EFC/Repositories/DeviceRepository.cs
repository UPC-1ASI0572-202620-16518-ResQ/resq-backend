using Microsoft.EntityFrameworkCore;
using ResQ.API.Device_Management.Domain.Model.Aggregates;
using ResQ.API.Device_Management.Domain.Model.Queries;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Device_Management.Infrastructure.Persistence.EFC.Repositories;

public class DeviceRepository(AppDbContext context) : BaseRepository<Device>(context), IDeviceRepository
{
    public async Task<Device?> FindByIdAndOrganizationIdAsync(Guid deviceId, Guid organizationId)
    {
        return await Context.Set<Device>().Include(device => device.Capabilities)
            .FirstOrDefaultAsync(device =>
                device.Id == deviceId &&
                device.OrganizationId == organizationId);
    }

    public async Task<bool> ExistsByOrganizationIdAndDeviceCodeAsync(Guid organizationId, DeviceCode deviceCode)
    {
        return await Context.Set<Device>()
            .AnyAsync(device =>
                device.OrganizationId == organizationId &&
                device.DeviceCode == deviceCode);
    }

    public async Task<Device?> FindByExternalReferenceAsync(Guid organizationId, string sourceSystem, string externalDeviceId)
    {
        return await Context.Set<Device>().Include(device => device.Capabilities)
            .FirstOrDefaultAsync(device =>
                device.OrganizationId == organizationId &&
                device.ExternalReference != null &&
                device.ExternalReference.SourceSystem == sourceSystem &&
                device.ExternalReference.ExternalDeviceId == externalDeviceId);
    }

    public async Task<bool> ExistsByExternalReferenceAsync(Guid organizationId, string sourceSystem, string externalDeviceId)
    {
        return await Context.Set<Device>()
            .AnyAsync(device =>
                device.OrganizationId == organizationId &&
                device.ExternalReference != null &&
                device.ExternalReference.SourceSystem == sourceSystem &&
                device.ExternalReference.ExternalDeviceId == externalDeviceId);
    }
    
    public async Task<PagedResult<Device>> FindPageAsync(Guid organizationId, Guid? buildingId, Guid? zoneId, EDeviceAdministrativeStatus? administrativeStatus, int page, int size)
    {
        var query = Context.Set<Device>().Include(device => device.Capabilities)
            .Where(device => device.OrganizationId == organizationId);

        if (buildingId.HasValue)
        {
            query = query.Where(device => device.Assignment.BuildingId == buildingId.Value);
        }

        if (zoneId.HasValue)
        {
            query = query.Where(device => device.Assignment.ZoneId == zoneId.Value);
        }

        if (administrativeStatus.HasValue)
        {
            query = query.Where(device => device.AdministrativeStatus == administrativeStatus.Value);
        }

        var totalElements = await query.CountAsync();

        var devices = await query
            .OrderBy(device => device.CreatedAt)
            .ThenBy(device => device.Id)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        var totalPages = totalElements == 0 ? 0 : (int)Math.Ceiling(totalElements / (double)size);

        return new PagedResult<Device>(devices, page, size, totalElements, totalPages);
    }
}