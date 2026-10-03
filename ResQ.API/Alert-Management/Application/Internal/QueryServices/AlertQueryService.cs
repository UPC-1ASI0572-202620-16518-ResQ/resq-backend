using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Queries;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;

namespace ResQ.API.Alert_Management.Application.Internal.QueryServices;

public class AlertQueryService(IAlertRepository alertRepository) : IAlertQueryService
{
    public async Task<Alert?> Handle(GetAlertByIdQuery query)
    {
        return await alertRepository.FindByIdAndOrganizationIdAsync(query.AlertId, query.OrganizationId);
    }

    public async Task<IEnumerable<Alert>> Handle(GetAlertsQuery query)
    {
        var riskTypeCode = string.IsNullOrWhiteSpace(query.RiskTypeCode)
            ? null
            : RiskCodes.NormalizeRiskTypeCode(query.RiskTypeCode);

        return await alertRepository.FindAllAsync(
            query.OrganizationId, query.BuildingId, query.ZoneId, riskTypeCode, query.From, query.To);
    }
}
