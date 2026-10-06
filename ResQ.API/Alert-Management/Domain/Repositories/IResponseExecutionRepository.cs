using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Domain.Repositories;

public interface IResponseExecutionRepository : IBaseRepository<ResponseExecution>
{
    Task<ResponseExecution?> FindByIdAndAlertIdAsync(Guid responseExecutionId, Guid alertId, Guid organizationId);

    Task<IEnumerable<ResponseExecution>> FindAllByAlertIdAsync(Guid alertId, Guid organizationId);
}
