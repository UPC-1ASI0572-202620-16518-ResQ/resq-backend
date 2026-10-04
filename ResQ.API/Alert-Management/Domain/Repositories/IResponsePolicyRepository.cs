using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Domain.Repositories;

public interface IResponsePolicyRepository : IBaseRepository<ResponsePolicy>
{
    Task<ResponsePolicy?> FindByIdAndOrganizationIdAsync(Guid policyId, Guid organizationId);

    Task<IEnumerable<ResponsePolicy>> FindAllAsync(Guid organizationId, string? riskTypeCode);

    Task<ResponsePolicy?> FindActiveByRiskTypeCodeAsync(Guid organizationId, string riskTypeCode);
}
