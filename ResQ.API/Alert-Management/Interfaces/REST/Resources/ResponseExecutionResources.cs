namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Response execution returned by the API. Matches ResponseExecutionResourceDto in the frontend
/// (plus alertId, which links the execution to the alert that triggered it).
/// </summary>
public record ResponseExecutionResource(
    Guid ResponseExecutionId,
    Guid OrganizationId,
    Guid AlertId,
    string RiskDetectionId,
    Guid PolicyId,
    ResponseActionSnapshotResource Action,
    string Status,
    DateTimeOffset RequestedAt,
    ResponseAuthorizationResource? Authorization,
    ExecutionResultResource? Result);

/// <summary>
/// Matches ResponseActionSnapshotResourceDto in the frontend. AuthorizationMode is AUTOMATIC or HUMAN_REQUIRED.
/// </summary>
public record ResponseActionSnapshotResource(
    Guid ActionId,
    string ActionCode,
    Guid TargetDeviceId,
    string TargetCapabilityCode,
    string AuthorizationMode,
    bool Critical);

/// <summary>
/// Matches ResponseAuthorizationResourceDto in the frontend. Decision is APPROVED or REJECTED.
/// </summary>
public record ResponseAuthorizationResource(
    Guid AuthorizationId,
    string Decision,
    string DecidedByUserId,
    DateTimeOffset DecidedAt);

/// <summary>
/// Matches ExecutionResultResourceDto in the frontend.
/// </summary>
public record ExecutionResultResource(
    bool Successful,
    string ResultCode,
    string? Message,
    DateTimeOffset CompletedAt);
