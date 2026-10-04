using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record ConfigureResponsePolicyCommand(
    Guid OrganizationId,
    string RiskTypeCode,
    IReadOnlyCollection<ResponseActionDefinition> Actions);
