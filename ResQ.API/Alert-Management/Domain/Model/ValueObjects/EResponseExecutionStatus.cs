namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Lifecycle of a response action execution.
/// </summary>
public enum EResponseExecutionStatus
{
    Pending,
    PendingAuthorization,
    Authorized,
    ExecutionRequested,
    Succeeded,
    Failed,
    Rejected
}
