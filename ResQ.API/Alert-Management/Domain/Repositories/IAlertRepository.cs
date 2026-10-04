using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Domain.Repositories;

public interface IAlertRepository : IBaseRepository<Alert>
{
    Task<Alert?> FindByIdAndOrganizationIdAsync(Guid alertId, Guid organizationId);

    Task<IEnumerable<Alert>> FindAllAsync(Guid organizationId, Guid? buildingId, Guid? zoneId, string? riskTypeCode,
        DateTimeOffset? from, DateTimeOffset? to);
}
