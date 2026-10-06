using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record DecideResponseAuthorizationCommand(
    Guid OrganizationId,
    Guid AlertId,
    Guid ResponseExecutionId,
    EAuthorizationDecision Decision,
    string DecidedByUserId);
