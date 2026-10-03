using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IAlertQueryService
{
    Task<Alert?> Handle(GetAlertByIdQuery query);

    Task<IEnumerable<Alert>> Handle(GetAlertsQuery query);
}
