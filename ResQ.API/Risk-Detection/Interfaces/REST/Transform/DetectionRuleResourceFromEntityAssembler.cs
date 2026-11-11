using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Interfaces.REST.Resources;

namespace ResQ.API.Risk_Detection.Interfaces.REST.Transform;

public static class DetectionRuleResourceFromEntityAssembler
{
    public static DetectionRuleResource ToResourceFromEntity(DetectionRule entity)
    {
        return new DetectionRuleResource(
            entity.Id,
            entity.RiskTypeCode,
            entity.SeverityCode,
            new DetectionConditionResource(
                entity.Condition.VariableType,
                EnumCode.ToCode(entity.Condition.Operator),
                DecimalFormat.Normalize(entity.Condition.Threshold)),
            EnumCode.ToCode(entity.Status));
    }
}
