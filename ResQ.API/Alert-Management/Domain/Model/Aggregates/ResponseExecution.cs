using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that tracks one response action requested for an alert
/// (e.g. closing a gas valve) and its human authorization when required.
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
    /// Action requested on the target device.
    /// </summary>
    public ResponseActionSnapshot Action { get; private set; } = null!;

    public EResponseExecutionStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public ResponseAuthorization? Authorization { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected ResponseExecution()
    {
    }

    /// <summary>
    /// Requests the execution of a response action. Automatic actions are sent to the actuator right away;
    /// actions that require a human decision wait in PENDING_AUTHORIZATION.
    /// </summary>
    public static ResponseExecution Request(Guid organizationId, Guid alertId, string riskDetectionId, ResponseActionSnapshot action)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        if (alertId == Guid.Empty)
            throw new ArgumentException("Alert id is required.");

        if (string.IsNullOrWhiteSpace(riskDetectionId))
            throw new ArgumentException("Risk detection id is required.");

        ArgumentNullException.ThrowIfNull(action);

        return new ResponseExecution
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            AlertId = alertId,
            RiskDetectionId = riskDetectionId.Trim(),
            Action = action,
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

        if (Authorization is not null || Status != EResponseExecutionStatus.PendingAuthorization)
            throw new InvalidOperationException("An authorization decision has already been registered.");

        Authorization = new ResponseAuthorization(decision, decidedByUserId);

        Status = decision == EAuthorizationDecision.Approved
            ? EResponseExecutionStatus.Authorized
            : EResponseExecutionStatus.Rejected;
    }
}
