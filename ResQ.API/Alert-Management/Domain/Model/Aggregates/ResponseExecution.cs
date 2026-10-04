using ResQ.API.Alert_Management.Domain.Model.Entities;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that tracks one response action requested for an alert,
/// including its human authorization (when required) and the actuator result.
/// </summary>
public class ResponseExecution
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Alert that triggered the execution.
    /// </summary>
    public Guid AlertId { get; private set; }

    /// <summary>
    /// Risk detection that originated the alert.
    /// </summary>
    public string RiskDetectionId { get; private set; } = string.Empty;

    /// <summary>
    /// Policy whose action is being executed.
    /// </summary>
    public Guid PolicyId { get; private set; }

    /// <summary>
    /// Copy of the policy action at request time.
    /// </summary>
    public ResponseActionSnapshot Action { get; private set; } = null!;

    public EResponseExecutionStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public ResponseAuthorization? Authorization { get; private set; }

    public ExecutionResult? Result { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected ResponseExecution()
    {
    }

    /// <summary>
    /// Requests the execution of a policy action. Automatic actions are sent to the actuator right away;
    /// actions that require a human decision wait in PENDING_AUTHORIZATION.
    /// </summary>
    public static ResponseExecution Request(Guid organizationId, Guid alertId, string riskDetectionId, Guid policyId, ResponseAction action)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        if (alertId == Guid.Empty)
            throw new ArgumentException("Alert id is required.");

        if (policyId == Guid.Empty)
            throw new ArgumentException("Policy id is required.");

        if (string.IsNullOrWhiteSpace(riskDetectionId))
            throw new ArgumentException("Risk detection id is required.");

        ArgumentNullException.ThrowIfNull(action);

        return new ResponseExecution
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            AlertId = alertId,
            RiskDetectionId = riskDetectionId.Trim(),
            PolicyId = policyId,
            Action = action.ToSnapshot(),
            Status = action.AuthorizationMode == EAuthorizationMode.HumanRequired
                ? EResponseExecutionStatus.PendingAuthorization
                : EResponseExecutionStatus.ExecutionRequested,
            RequestedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Registers the human decision for an execution that requires authorization.
    /// </summary>
    public void DecideAuthorization(EAuthorizationDecision decision, string decidedByUserId)
    {
        if (Action.AuthorizationMode != EAuthorizationMode.HumanRequired)
            throw new InvalidOperationException("This response execution does not require human authorization.");

        if (Authorization is not null)
            throw new InvalidOperationException("An authorization decision has already been registered.");

        if (Status != EResponseExecutionStatus.PendingAuthorization)
            throw new InvalidOperationException("A decision can only be registered while the response execution is pending authorization.");

        Authorization = new ResponseAuthorization(decision, decidedByUserId);

        Status = decision == EAuthorizationDecision.Approved
            ? EResponseExecutionStatus.Authorized
            : EResponseExecutionStatus.Rejected;
    }

    /// <summary>
    /// Records the outcome reported by the actuator.
    /// </summary>
    public void RecordResult(bool successful, string resultCode, string? message)
    {
        if (Status != EResponseExecutionStatus.ExecutionRequested && Status != EResponseExecutionStatus.Authorized)
            throw new InvalidOperationException("A result can only be recorded for an authorized or requested execution.");

        Result = new ExecutionResult(successful, resultCode, message);

        Status = successful
            ? EResponseExecutionStatus.Succeeded
            : EResponseExecutionStatus.Failed;
    }
}
