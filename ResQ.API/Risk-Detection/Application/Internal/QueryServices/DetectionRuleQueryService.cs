using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Queries;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;

namespace ResQ.API.Risk_Detection.Application.Internal.QueryServices;

public class DetectionRuleQueryService(IDetectionRuleRepository detectionRuleRepository) : IDetectionRuleQueryService
{
    public async Task<DetectionRule?> Handle(GetDetectionRuleByIdQuery query)
    {
        return await detectionRuleRepository.FindByIdAsync(query.OrganizationId, query.RuleId);
    }

    public async Task<IEnumerable<DetectionRule>> Handle(GetDetectionRulesQuery query)
    {
        var variableType = string.IsNullOrWhiteSpace(query.VariableType)
            ? null
            : RiskCodes.NormalizeVariableType(query.VariableType);

        return await detectionRuleRepository.FindAllAsync(query.OrganizationId, query.Status, variableType);
    }
}
