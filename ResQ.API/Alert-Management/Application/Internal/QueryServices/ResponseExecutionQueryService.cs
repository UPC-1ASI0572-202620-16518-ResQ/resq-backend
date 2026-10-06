using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;

namespace ResQ.API.Alert_Management.Application.Internal.QueryServices;

public class ResponseExecutionQueryService(IResponseExecutionRepository responseExecutionRepository) : IResponseExecutionQueryService
{
    public async Task<IEnumerable<ResponseExecution>> Handle(GetResponseExecutionsByAlertIdQuery query)
    {
        return await responseExecutionRepository.FindAllByAlertIdAsync(query.AlertId, query.OrganizationId);
    }
}
