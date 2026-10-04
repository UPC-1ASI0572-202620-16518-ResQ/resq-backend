using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record UpdateResponsePolicyCommand(
    Guid OrganizationId,
    Guid PolicyId,
    string RiskTypeCode,
    IReadOnlyCollection<ResponseActionDefinition> Actions);
