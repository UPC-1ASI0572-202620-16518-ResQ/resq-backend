using Microsoft.EntityFrameworkCore;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Risk_Detection.Infrastructure.Persistence.EFC.Repositories;

public class DetectionRuleRepository(AppDbContext context)
    : BaseRepository<DetectionRule>(context), IDetectionRuleRepository
{
    public async Task<DetectionRule?> FindByIdAsync(Guid organizationId, Guid ruleId)
    {
        return await Context.Set<DetectionRule>()
            .FirstOrDefaultAsync(rule => rule.Id == ruleId && rule.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<DetectionRule>> FindActiveByVariableTypeAsync(Guid organizationId, string variableType)
    {
        return await Context.Set<DetectionRule>()
            .Where(rule =>
                rule.OrganizationId == organizationId &&
                rule.Status == EDetectionRuleStatus.Active &&
                rule.Condition.VariableType == variableType)
            .ToListAsync();
    }

    public async Task<IEnumerable<DetectionRule>> FindAllAsync(Guid organizationId, EDetectionRuleStatus? status, string? variableType)
    {
        var query = Context.Set<DetectionRule>().Where(rule => rule.OrganizationId == organizationId);

        if (status.HasValue)
            query = query.Where(rule => rule.Status == status.Value);

        if (variableType is not null)
            query = query.Where(rule => rule.Condition.VariableType == variableType);

        return await query
            .OrderBy(rule => rule.ConfiguredAt)
            .ToListAsync();
    }
}
