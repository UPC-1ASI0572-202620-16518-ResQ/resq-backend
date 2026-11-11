using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Queries;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;

namespace ResQ.API.Risk_Detection.Application.Internal.QueryServices;

public class RiskDetectionQueryService(IRiskDetectionRepository riskDetectionRepository) : IRiskDetectionQueryService
{
    public async Task<RiskDetection?> Handle(GetRiskDetectionQuery query)
    {
        return await riskDetectionRepository.FindByIdAsync(query.OrganizationId, query.RiskDetectionId);
    }

    public async Task<RiskDetection?> Handle(GetRiskDetectionEvidenceQuery query)
    {
        return await riskDetectionRepository.FindByIdAsync(query.OrganizationId, query.RiskDetectionId);
    }

    public async Task<IEnumerable<RiskDetection>> Handle(GetRiskDetectionsQuery query)
    {
        var riskTypeCode = string.IsNullOrWhiteSpace(query.RiskTypeCode)
            ? null
            : RiskCodes.NormalizeRiskTypeCode(query.RiskTypeCode);

        return await riskDetectionRepository.FindAllAsync(query.OrganizationId, query.BuildingId, query.ZoneId, riskTypeCode);
    }
}
