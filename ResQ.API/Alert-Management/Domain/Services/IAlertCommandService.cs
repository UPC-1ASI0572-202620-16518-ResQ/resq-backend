using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;

namespace ResQ.API.Alert_Management.Domain.Services;

public interface IAlertCommandService
{
    /// <summary>
    /// Generates an alert and requests the response executions of the active policy for its risk type.
    /// </summary>
    Task<Alert?> Handle(GenerateAlertCommand command);

    Task<Alert?> Handle(RecordNotificationDeliveryOutcomeCommand command);
}
