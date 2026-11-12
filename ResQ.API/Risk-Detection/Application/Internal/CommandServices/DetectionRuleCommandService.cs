using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Risk_Detection.Application.Internal.CommandServices;

public class DetectionRuleCommandService(
    IDetectionRuleRepository detectionRuleRepository,
    IUnitOfWork unitOfWork) : IDetectionRuleCommandService
{
    public async Task<DetectionRule> Handle(ConfigureDetectionRuleCommand command)
    {
        var condition = DetectionCondition.Create(command.VariableType, command.Operator, command.Threshold);
        var rule = DetectionRule.Configure(
            command.OrganizationId, command.RiskTypeCode, command.SeverityCode, condition, command.ZoneId);

        await detectionRuleRepository.AddAsync(rule);
        await unitOfWork.CompleteAsync();

        return rule;
    }

    public async Task<DetectionRule?> Handle(ChangeDetectionRuleStatusCommand command)
    {
        var rule = await detectionRuleRepository.FindByIdAsync(command.OrganizationId, command.RuleId);

        if (rule is null) return null;

        if (command.Active) rule.Activate();
        else rule.Deactivate();

        await unitOfWork.CompleteAsync();

        return rule;
    }
}
