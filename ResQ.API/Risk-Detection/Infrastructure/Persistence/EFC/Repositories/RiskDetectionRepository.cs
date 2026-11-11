using Microsoft.EntityFrameworkCore;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Risk_Detection.Infrastructure.Persistence.EFC.Repositories;

public class RiskDetectionRepository(AppDbContext context)
    : BaseRepository<RiskDetection>(context), IRiskDetectionRepository
{
    private IQueryable<RiskDetection> WithDetails() =>
        Context.Set<RiskDetection>()
            .Include(detection => detection.Evidence)
            .Include(detection => detection.SeverityChanges)
            .AsSplitQuery();

    public async Task<RiskDetection?> FindByIdAsync(Guid organizationId, Guid riskDetectionId)
    {
        return await WithDetails()
            .FirstOrDefaultAsync(detection => detection.Id == riskDetectionId && detection.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<RiskDetection>> FindAllAsync(Guid organizationId, Guid? buildingId, Guid? zoneId, string? riskTypeCode)
    {
        var query = WithDetails().Where(detection => detection.OrganizationId == organizationId);

        if (buildingId.HasValue)
            query = query.Where(detection => detection.Location.BuildingId == buildingId.Value);

        if (zoneId.HasValue)
            query = query.Where(detection => detection.Location.ZoneId == zoneId.Value);

        if (riskTypeCode is not null)
            query = query.Where(detection => detection.RiskTypeCode == riskTypeCode);

        // Most recent detections first
        return await query
            .OrderByDescending(detection => detection.DetectedAt)
            .ToListAsync();
    }

    public async Task<RiskDetection?> FindOpenAsync(Guid organizationId, string riskTypeCode, RiskLocation location, DateTimeOffset since)
    {
        var buildingId = location.BuildingId;
        var zoneId = location.ZoneId;

        return await WithDetails()
            .Where(detection =>
                detection.OrganizationId == organizationId &&
                detection.RiskTypeCode == riskTypeCode &&
                detection.Location.ResolutionStatus == location.ResolutionStatus &&
                detection.Location.BuildingId == buildingId &&
                detection.Location.ZoneId == zoneId &&
                detection.LastEvidenceAt >= since)
            .OrderByDescending(detection => detection.LastEvidenceAt)
            .FirstOrDefaultAsync();
    }
}
