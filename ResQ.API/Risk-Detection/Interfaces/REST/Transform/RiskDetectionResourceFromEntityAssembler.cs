using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Interfaces.REST.Resources;

namespace ResQ.API.Risk_Detection.Interfaces.REST.Transform;

public static class RiskDetectionResourceFromEntityAssembler
{
    public static RiskDetectionResource ToResourceFromEntity(RiskDetection entity)
    {
        return new RiskDetectionResource(
            entity.Id,
            entity.RuleId,
            entity.RiskTypeCode,
            entity.CurrentSeverityCode,
            new RiskLocationResource(
                entity.Location.BuildingId,
                entity.Location.ZoneId,
                EnumCode.ToCode(entity.Location.ResolutionStatus)),
            entity.DetectedAt,
            entity.Evidence
                .OrderBy(evidence => evidence.MeasuredAt)
                .Select(evidence => new DetectionEvidenceResource(
                    evidence.MeasurementId,
                    evidence.DeviceId,
                    evidence.VariableType,
                    DecimalFormat.Normalize(evidence.Value),
                    evidence.MeasuredAt))
                .ToList(),
            entity.SeverityChanges
                .OrderBy(change => change.ChangedAt)
                .Select(change => new SeverityChangeResource(
                    change.Id,
                    change.PreviousSeverityCode,
                    change.NewSeverityCode,
                    change.ChangedAt))
                .ToList());
    }

    public static MeasurementEvaluationResource ToResourceFromResult(MeasurementEvaluationResult result)
    {
        return new MeasurementEvaluationResource(
            result.MeasurementId,
            result.EvaluatedRules,
            result.RiskDetections.Count > 0,
            result.RiskDetections.Select(ToResourceFromEntity).ToList(),
            result.GeneratedAlertIds);
    }
}
