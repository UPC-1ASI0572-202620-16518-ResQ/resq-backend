using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Alert_Management.Infrastructure.Persistence.EFC.Repositories;

public class ResponsePolicyRepository(AppDbContext context) : BaseRepository<ResponsePolicy>(context), IResponsePolicyRepository
{
    public async Task<ResponsePolicy?> FindByIdAndOrganizationIdAsync(Guid policyId, Guid organizationId)
    {
        return await Context.Set<ResponsePolicy>().Include(policy => policy.Actions)
            .FirstOrDefaultAsync(policy =>
                policy.Id == policyId &&
                policy.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<ResponsePolicy>> FindAllAsync(Guid organizationId, string? riskTypeCode)
    {
        var query = Context.Set<ResponsePolicy>().Include(policy => policy.Actions)
            .Where(policy => policy.OrganizationId == organizationId);

        if (riskTypeCode is not null)
            query = query.Where(policy => policy.RiskTypeCode == riskTypeCode);

        return await query
            .OrderBy(policy => policy.RiskTypeCode)
            .ThenBy(policy => policy.CreatedAt)
            .ToListAsync();
    }

    public async Task<ResponsePolicy?> FindActiveByRiskTypeCodeAsync(Guid organizationId, string riskTypeCode)
    {
        return await Context.Set<ResponsePolicy>().Include(policy => policy.Actions)
            .FirstOrDefaultAsync(policy =>
                policy.OrganizationId == organizationId &&
                policy.RiskTypeCode == riskTypeCode &&
                policy.Status == EResponsePolicyStatus.Active);
    }
}
