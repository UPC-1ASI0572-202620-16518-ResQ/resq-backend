using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Commands;

namespace ResQ.API.Risk_Detection.Domain.Services;

public interface IDetectionRuleCommandService
{
    Task<DetectionRule> Handle(ConfigureDetectionRuleCommand command);

    /// <returns>The rule, or null when it does not exist.</returns>
    Task<DetectionRule?> Handle(ChangeDetectionRuleStatusCommand command);
}
