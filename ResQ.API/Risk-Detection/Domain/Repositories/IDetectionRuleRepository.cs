using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Risk_Detection.Domain.Repositories;

public interface IDetectionRuleRepository : IBaseRepository<DetectionRule>
{
    Task<DetectionRule?> FindByIdAsync(Guid organizationId, Guid ruleId);

    Task<IEnumerable<DetectionRule>> FindActiveByVariableTypeAsync(Guid organizationId, string variableType, Guid? zoneId);

    Task<IEnumerable<DetectionRule>> FindByZoneAsync(Guid organizationId, Guid zoneId);

    Task<IEnumerable<DetectionRule>> FindAllAsync(Guid organizationId, EDetectionRuleStatus? status, string? variableType);
}
