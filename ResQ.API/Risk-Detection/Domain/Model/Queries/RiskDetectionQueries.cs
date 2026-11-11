using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Domain.Model.Queries;

public record GetDetectionRuleByIdQuery(Guid OrganizationId, Guid RuleId);

public record GetDetectionRulesQuery(Guid OrganizationId, EDetectionRuleStatus? Status, string? VariableType);

public record GetRiskDetectionQuery(Guid OrganizationId, Guid RiskDetectionId);

/// <summary>
/// Retrieves a detection together with its evidence and severity traceability.
/// </summary>
public record GetRiskDetectionEvidenceQuery(Guid OrganizationId, Guid RiskDetectionId);

public record GetRiskDetectionsQuery(Guid OrganizationId, Guid? BuildingId, Guid? ZoneId, string? RiskTypeCode);
