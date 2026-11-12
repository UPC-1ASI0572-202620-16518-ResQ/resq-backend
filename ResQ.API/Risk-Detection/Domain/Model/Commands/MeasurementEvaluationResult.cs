using ResQ.API.Risk_Detection.Domain.Model.Aggregates;

namespace ResQ.API.Risk_Detection.Domain.Model.Commands;

/// <summary>
/// Result of evaluating a measurement: the detections created or updated and the alerts raised downstream.
/// </summary>
public record MeasurementEvaluationResult(
    Guid MeasurementId,
    int EvaluatedRules,
    IReadOnlyCollection<RiskDetection> RiskDetections,
    IReadOnlyCollection<Guid> GeneratedAlertIds,
    IReadOnlyCollection<Guid> GeneratedIncidentIds);
