using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Queries;

namespace ResQ.API.Risk_Detection.Domain.Services;

public interface IRiskDetectionQueryService
{
    Task<RiskDetection?> Handle(GetRiskDetectionQuery query);

    Task<RiskDetection?> Handle(GetRiskDetectionEvidenceQuery query);

    Task<IEnumerable<RiskDetection>> Handle(GetRiskDetectionsQuery query);
}
