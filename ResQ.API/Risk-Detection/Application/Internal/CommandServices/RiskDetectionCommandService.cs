using Microsoft.Extensions.Options;
using ResQ.API.Risk_Detection.Application.Internal.OutboundServices;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.Entities;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Domain.Repositories;
using ResQ.API.Risk_Detection.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Risk_Detection.Application.Internal.CommandServices;

/// <summary>
/// Evaluates valid monitored measurements against the active detection rules (MeasurementReceived flow in Cloud).
/// </summary>
public class RiskDetectionCommandService(
    IDetectionRuleRepository detectionRuleRepository,
    IRiskDetectionRepository riskDetectionRepository,
    IRiskLocationResolver riskLocationResolver,
    IRiskDetectionEventPublisher riskDetectionEventPublisher,
    IOptions<RiskDetectionSettings> settings,
    IUnitOfWork unitOfWork) : IRiskDetectionCommandService
{
    public async Task<MeasurementEvaluationResult> Handle(EvaluateMeasurementCommand command)
    {
        // Validates the measurement as evidence (ids, variable type, measurement date)
        var probe = DetectionEvidence.Create(
            command.MeasurementId, command.DeviceId, command.VariableType, command.Value, command.MeasuredAt);

        var rules = (await detectionRuleRepository.FindActiveByVariableTypeAsync(command.OrganizationId, probe.VariableType)).ToList();
        var satisfiedRules = RiskEvaluationService.SelectSatisfiedRules(rules, probe);

        if (satisfiedRules.Count == 0)
            return new MeasurementEvaluationResult(command.MeasurementId, rules.Count, [], []);

        var location = await riskLocationResolver.ResolveAsync(command.OrganizationId, command.DeviceId);
        var now = DateTimeOffset.UtcNow;
        var detections = new List<RiskDetection>();

        foreach (var rule in satisfiedRules)
        {
            // Each detection keeps its own copy of the evidence
            var evidence = DetectionEvidence.Create(
                command.MeasurementId, command.DeviceId, command.VariableType, command.Value, command.MeasuredAt);

            var existing = await riskDetectionRepository.FindOpenAsync(
                command.OrganizationId, rule.RiskTypeCode, location, probe.MeasuredAt - settings.Value.CorrelationWindow);

            if (existing is null)
            {
                var detection = RiskDetection.Detect(rule, location, evidence);
                await riskDetectionRepository.AddAsync(detection);
                detections.Add(detection);
            }
            else
            {
                existing.AddEvidence(evidence);
                existing.UpdateSeverity(rule.Id, rule.SeverityCode, now);
                detections.Add(existing);
            }
        }

        await unitOfWork.CompleteAsync();

        // The detection result continues through the distributed flow (Alert & Response)
        var alertIds = settings.Value.RaiseAlerts
            ? await riskDetectionEventPublisher.PublishAsync(command.OrganizationId, detections)
            : [];

        return new MeasurementEvaluationResult(command.MeasurementId, rules.Count, detections, alertIds);
    }
}
