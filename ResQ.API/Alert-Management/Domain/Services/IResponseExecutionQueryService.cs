using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IResponseExecutionQueryService
{
    Task<IEnumerable<ResponseExecution>> Handle(GetResponseExecutionsByAlertIdQuery query);
}
