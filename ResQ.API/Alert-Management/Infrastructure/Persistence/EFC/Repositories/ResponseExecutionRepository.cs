using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Alert_Management.Infrastructure.Persistence.EFC.Repositories;

public class ResponseExecutionRepository(AppDbContext context) : BaseRepository<ResponseExecution>(context), IResponseExecutionRepository
{
    public async Task<ResponseExecution?> FindByIdAndOrganizationIdAsync(Guid responseExecutionId, Guid organizationId)
    {
        return await Context.Set<ResponseExecution>()
            .FirstOrDefaultAsync(execution =>
                execution.Id == responseExecutionId &&
                execution.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<ResponseExecution>> FindAllAsync(Guid organizationId, string? riskDetectionId, Guid? alertId,
        EResponseExecutionStatus? status, DateTimeOffset? from, DateTimeOffset? to)
    {
        var query = Context.Set<ResponseExecution>()
            .Where(execution => execution.OrganizationId == organizationId);

        if (riskDetectionId is not null)
            query = query.Where(execution => execution.RiskDetectionId == riskDetectionId);

        if (alertId.HasValue)
            query = query.Where(execution => execution.AlertId == alertId.Value);

        if (status.HasValue)
            query = query.Where(execution => execution.Status == status.Value);

        if (from.HasValue)
            query = query.Where(execution => execution.RequestedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(execution => execution.RequestedAt <= to.Value);

        // Most recent executions first
        return await query
            .OrderByDescending(execution => execution.RequestedAt)
            .ToListAsync();
    }
}
