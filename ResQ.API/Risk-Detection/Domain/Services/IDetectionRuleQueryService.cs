using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Queries;

namespace ResQ.API.Risk_Detection.Domain.Services;

public interface IDetectionRuleQueryService
{
    Task<DetectionRule?> Handle(GetDetectionRuleByIdQuery query);

    Task<IEnumerable<DetectionRule>> Handle(GetDetectionRulesQuery query);
}
