using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;

namespace ResQ.API.Alert_Management.Application.Internal.QueryServices;

public class ResponsePolicyQueryService(IResponsePolicyRepository responsePolicyRepository) : IResponsePolicyQueryService
{
    public async Task<ResponsePolicy?> Handle(GetResponsePolicyByIdQuery query)
    {
        return await responsePolicyRepository.FindByIdAndOrganizationIdAsync(query.PolicyId, query.OrganizationId);
    }

    public async Task<IEnumerable<ResponsePolicy>> Handle(GetResponsePoliciesQuery query)
    {
        var riskTypeCode = string.IsNullOrWhiteSpace(query.RiskTypeCode)
            ? null
            : RiskCodes.NormalizeRiskTypeCode(query.RiskTypeCode);

        return await responsePolicyRepository.FindAllAsync(query.OrganizationId, riskTypeCode);
    }
}
