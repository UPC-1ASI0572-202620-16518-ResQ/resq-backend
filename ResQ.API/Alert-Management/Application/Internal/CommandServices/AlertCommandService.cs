using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Building_Management.Interfaces.ACL;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Application.Internal.CommandServices;

public class AlertCommandService(
    IAlertRepository alertRepository,
    IResponsePolicyRepository responsePolicyRepository,
    IResponseExecutionRepository responseExecutionRepository,
    IBuildingsContextFacade buildingsContextFacade,
    IUnitOfWork unitOfWork)
    : IAlertCommandService
{
    public async Task<Alert?> Handle(GenerateAlertCommand command)
    {
        var context = AlertContext.Create(
            command.RiskDetectionId,
            command.RiskTypeCode,
            command.SeverityCode,
            command.BuildingId,
            command.ZoneId,
            command.DetectedAt);

        // The location is validated through the Building Management ACL
        if (context.BuildingId.HasValue)
        {
            var validLocation = await buildingsContextFacade.ValidateAssignmentAsync(
                command.OrganizationId, context.BuildingId.Value, context.ZoneId);

            if (!validLocation)
                throw new InvalidOperationException("The building or zone does not exist or is not active.");
        }

        var alert = Alert.Generate(command.OrganizationId, context, command.Recipients);

        await alertRepository.AddAsync(alert);

        // The active policy for the risk type defines which response actions are requested
        var policy = await responsePolicyRepository.FindActiveByRiskTypeCodeAsync(command.OrganizationId, context.RiskTypeCode);

        if (policy is not null)
        {
            foreach (var action in policy.Actions)
            {
                var execution = ResponseExecution.Request(
                    command.OrganizationId, alert.Id, context.RiskDetectionId, policy.Id, action);

                await responseExecutionRepository.AddAsync(execution);
            }
        }

        await unitOfWork.CompleteAsync();

        return alert;
    }

    public async Task<Alert?> Handle(RecordNotificationDeliveryOutcomeCommand command)
    {
        var alert = await alertRepository.FindByIdAndOrganizationIdAsync(command.AlertId, command.OrganizationId);

        if (alert is null)
            throw new KeyNotFoundException("Alert not found.");

        alert.RecordDeliveryOutcome(command.DeliveryId, command.Status, command.FailureReason);

        await unitOfWork.CompleteAsync();

        return alert;
    }
}
