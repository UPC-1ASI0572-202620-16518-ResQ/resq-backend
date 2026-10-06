using Microsoft.EntityFrameworkCore;
using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Building_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Building_Management.Infrastructure.Persistence.EFC.Repositories;

public class BuildingRepository(AppDbContext context)
    : BaseRepository<Building>(context), IBuildingRepository
{
    public async Task<Building?> FindByIdAndOrganizationIdAsync(Guid buildingId, Guid organizationId)
    {
        return await Context.Set<Building>()
            .Include(b => b.Zones)
            .FirstOrDefaultAsync(b =>
                b.Id == buildingId &&
                b.OrganizationId == organizationId);
    }

    public async Task<bool> ExistsByOrganizationIdAndBuildingCodeAsync(
        Guid organizationId, BuildingCode buildingCode)
    {
        return await Context.Set<Building>()
            .AnyAsync(b =>
                b.OrganizationId == organizationId &&
                b.BuildingCode == buildingCode);
    }

    public async Task<PagedResult<Building>> FindPageAsync(
        Guid organizationId,
        LocationAdministrativeStatus? administrativeStatus,
        int page, int size)
    {
        var query = Context.Set<Building>()
            .Include(b => b.Zones)
            .Where(b => b.OrganizationId == organizationId);

        if (administrativeStatus.HasValue)
            query = query.Where(b => b.AdministrativeStatus == administrativeStatus.Value);

        var totalElements = await query.CountAsync();

        var buildings = await query
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        var totalPages = totalElements == 0 ? 0 : (int)Math.Ceiling(totalElements / (double)size);

        return new PagedResult<Building>(buildings, page, size, totalElements, totalPages);
    }

    public async Task<PagedResult<Zone>> FindZonesPageAsync(
        Guid organizationId,
        Guid buildingId,
        LocationAdministrativeStatus? administrativeStatus,
        int page, int size)
    {
        // First verify building belongs to organization
        var buildingExists = await Context.Set<Building>()
            .AnyAsync(b => b.Id == buildingId && b.OrganizationId == organizationId);

        if (!buildingExists)
            return new PagedResult<Zone>(Array.Empty<Zone>(), page, size, 0, 0);

        var query = Context.Set<Zone>()
            .Where(z => EF.Property<Guid>(z, "BuildingId") == buildingId);

        if (administrativeStatus.HasValue)
            query = query.Where(z => z.AdministrativeStatus == administrativeStatus.Value);

        var totalElements = await query.CountAsync();

        var zones = await query
            .OrderBy(z => z.CreatedAt)
            .ThenBy(z => z.Id)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        var totalPages = totalElements == 0 ? 0 : (int)Math.Ceiling(totalElements / (double)size);

        return new PagedResult<Zone>(zones, page, size, totalElements, totalPages);
    }

    public async Task<Zone?> FindZoneByIdAsync(Guid organizationId, Guid buildingId, Guid zoneId)
    {
        var building = await Context.Set<Building>()
            .Include(b => b.Zones)
            .FirstOrDefaultAsync(b =>
                b.Id == buildingId &&
                b.OrganizationId == organizationId);

        return building?.Zones.FirstOrDefault(z => z.Id == zoneId);
    }

    public async Task<Building?> FindByZoneIdAndOrganizationIdAsync(Guid zoneId, Guid organizationId)
    {
        return await Context.Set<Building>()
            .Include(b => b.Zones)
            .FirstOrDefaultAsync(b =>
                b.OrganizationId == organizationId &&
                b.Zones.Any(z => z.Id == zoneId));
    }
}
