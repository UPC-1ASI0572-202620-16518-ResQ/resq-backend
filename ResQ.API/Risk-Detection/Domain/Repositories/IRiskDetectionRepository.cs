using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Risk_Detection.Domain.Repositories;

public interface IRiskDetectionRepository : IBaseRepository<RiskDetection>
{
    /// <summary>
    /// Finds a detection with its evidence and severity changes.
    /// </summary>
    Task<RiskDetection?> FindByIdAsync(Guid organizationId, Guid riskDetectionId);

    Task<IEnumerable<RiskDetection>> FindAllAsync(Guid organizationId, Guid? buildingId, Guid? zoneId, string? riskTypeCode);

    /// <summary>
    /// Finds the most recent detection of the same risk type and location whose last evidence is not older than <paramref name="since"/>.
    /// </summary>
    Task<RiskDetection?> FindOpenAsync(Guid organizationId, string riskTypeCode, RiskLocation location, DateTimeOffset since);
}
