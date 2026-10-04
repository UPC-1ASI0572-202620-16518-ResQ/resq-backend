using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IResponseExecutionQueryService
{
    Task<ResponseExecution?> Handle(GetResponseExecutionByIdQuery query);

    Task<IEnumerable<ResponseExecution>> Handle(GetResponseExecutionsQuery query);
}
