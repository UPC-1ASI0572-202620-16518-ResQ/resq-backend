using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Domain.Repositories;

public interface IResponseExecutionRepository : IBaseRepository<ResponseExecution>
{
    Task<ResponseExecution?> FindByIdAndOrganizationIdAsync(Guid responseExecutionId, Guid organizationId);

    Task<IEnumerable<ResponseExecution>> FindAllAsync(Guid organizationId, string? riskDetectionId, Guid? alertId,
        EResponseExecutionStatus? status, DateTimeOffset? from, DateTimeOffset? to);
}
