using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Alert_Management.Infrastructure.Persistence.EFC.Repositories;

public class ResponseExecutionRepository(AppDbContext context) : BaseRepository<ResponseExecution>(context), IResponseExecutionRepository
{
    public async Task<ResponseExecution?> FindByIdAndAlertIdAsync(Guid responseExecutionId, Guid alertId, Guid organizationId)
    {
        return await Context.Set<ResponseExecution>()
            .FirstOrDefaultAsync(execution =>
                execution.Id == responseExecutionId &&
                execution.AlertId == alertId &&
                execution.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<ResponseExecution>> FindAllByAlertIdAsync(Guid alertId, Guid organizationId)
    {
        return await Context.Set<ResponseExecution>()
            .Where(execution => execution.AlertId == alertId && execution.OrganizationId == organizationId)
            .OrderBy(execution => execution.RequestedAt)
            .ToListAsync();
    }
}
