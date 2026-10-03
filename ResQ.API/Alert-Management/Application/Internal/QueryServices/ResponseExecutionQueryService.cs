using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;

namespace ResQ.API.Alert_Management.Application.Internal.QueryServices;

public class ResponseExecutionQueryService(IResponseExecutionRepository responseExecutionRepository) : IResponseExecutionQueryService
{
    public async Task<ResponseExecution?> Handle(GetResponseExecutionByIdQuery query)
    {
        return await responseExecutionRepository.FindByIdAndOrganizationIdAsync(query.ResponseExecutionId, query.OrganizationId);
    }

    public async Task<IEnumerable<ResponseExecution>> Handle(GetResponseExecutionsQuery query)
    {
        var riskDetectionId = string.IsNullOrWhiteSpace(query.RiskDetectionId) ? null : query.RiskDetectionId.Trim();

        return await responseExecutionRepository.FindAllAsync(
            query.OrganizationId, riskDetectionId, query.AlertId, query.Status, query.From, query.To);
    }
}
