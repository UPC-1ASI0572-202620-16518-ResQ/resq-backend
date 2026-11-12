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

    public async Task<IEnumerable<DetectionRule>> FindActiveByVariableTypeAsync(
        Guid organizationId, string variableType, Guid? zoneId)
    {
        var candidates = await Context.Set<DetectionRule>()
            .Where(rule =>
                rule.OrganizationId == organizationId &&
                rule.Status == EDetectionRuleStatus.Active &&
                rule.Condition.VariableType == variableType &&
                (rule.ZoneId == null || rule.ZoneId == zoneId))
            .ToListAsync();

        var scoped = zoneId.HasValue ? candidates.Where(rule => rule.ZoneId == zoneId).ToList() : [];
        return scoped.Count > 0 ? scoped : candidates.Where(rule => rule.ZoneId == null).ToList();
    }

    public async Task<IEnumerable<DetectionRule>> FindByZoneAsync(Guid organizationId, Guid zoneId)
    {
        return await Context.Set<DetectionRule>().Where(rule =>
            rule.OrganizationId == organizationId && rule.ZoneId == zoneId)
            .OrderBy(rule => rule.ConfiguredAt).ToListAsync();
    }

    public async Task<IEnumerable<DetectionRule>> FindAllAsync(Guid organizationId, EDetectionRuleStatus? status, string? variableType)
    {
        var query = Context.Set<DetectionRule>().Where(rule =>
            rule.OrganizationId == organizationId && rule.ZoneId == null);

        if (status.HasValue)
            query = query.Where(rule => rule.Status == status.Value);

        if (variableType is not null)
            query = query.Where(rule => rule.Condition.VariableType == variableType);

        return await query
            .OrderBy(rule => rule.ConfiguredAt)
            .ToListAsync();
    }
}
