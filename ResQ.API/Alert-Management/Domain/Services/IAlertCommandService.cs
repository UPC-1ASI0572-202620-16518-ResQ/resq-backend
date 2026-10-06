using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IAlertCommandService
{
    /// <summary>
    /// Generates an alert, its notification deliveries and the requested response executions.
    /// </summary>
    Task<Alert?> Handle(GenerateAlertCommand command);
}
