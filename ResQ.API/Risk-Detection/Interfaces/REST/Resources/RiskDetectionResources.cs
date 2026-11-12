namespace ResQ.API.Risk_Detection.Interfaces.REST.Resources;

/// <summary>
/// Risk detection returned by the API. Matches RiskDetectionResourceDto in the frontend.
/// </summary>
public record RiskDetectionResource(
    Guid RiskDetectionId,
    Guid RuleId,
    string RiskTypeCode,
    string CurrentSeverityCode,
    RiskLocationResource Location,
    DateTimeOffset DetectedAt,
    IEnumerable<DetectionEvidenceResource> Evidence,
    IEnumerable<SeverityChangeResource> SeverityChanges);

/// <summary>
/// Matches RiskLocationResourceDto in the frontend. ResolutionStatus is RESOLVED or UNRESOLVED.
/// </summary>
public record RiskLocationResource(
    Guid? BuildingId,
    Guid? ZoneId,
    string ResolutionStatus);

/// <summary>
/// Matches DetectionEvidenceResourceDto in the frontend.
/// </summary>
public record DetectionEvidenceResource(
    Guid MeasurementId,
    Guid DeviceId,
    string VariableType,
    decimal Value,
    DateTimeOffset MeasuredAt);

/// <summary>
/// Matches SeverityChangeResourceDto in the frontend.
/// </summary>
public record SeverityChangeResource(
    Guid SeverityChangeId,
    string PreviousSeverityCode,
    string NewSeverityCode,
    DateTimeOffset ChangedAt);

/// <summary>
/// Valid monitored measurement to evaluate. MeasurementId is optional while Monitoring does not exist
/// (a new id is generated when omitted).
/// </summary>
public record EvaluateMeasurementResource(
    Guid DeviceId,
    string VariableType,
    decimal? Value,
    DateTimeOffset MeasuredAt,
    Guid? MeasurementId);

/// <summary>
/// Result of a measurement evaluation.
/// </summary>
public record MeasurementEvaluationResource(
    Guid MeasurementId,
    int EvaluatedRules,
    bool RiskDetected,
    IEnumerable<RiskDetectionResource> RiskDetections,
    IEnumerable<Guid> GeneratedAlertIds,
    IEnumerable<Guid> GeneratedIncidentIds);
