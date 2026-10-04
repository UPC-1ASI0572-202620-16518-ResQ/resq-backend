namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Copy of the policy action at the moment the execution was requested,
/// so later policy changes do not alter the execution history.
/// </summary>
public record ResponseActionSnapshot
{
    public Guid ActionId { get; init; }

    public string ActionCode { get; init; } = string.Empty;

    public Guid TargetDeviceId { get; init; }

    public string TargetCapabilityCode { get; init; } = string.Empty;

    public EAuthorizationMode AuthorizationMode { get; init; }

    public bool Critical { get; init; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    private ResponseActionSnapshot()
    {
    }

    public ResponseActionSnapshot(Guid actionId, string actionCode, Guid targetDeviceId, string targetCapabilityCode,
        EAuthorizationMode authorizationMode, bool critical)
    {
        ActionId = actionId;
        ActionCode = actionCode;
        TargetDeviceId = targetDeviceId;
        TargetCapabilityCode = targetCapabilityCode;
        AuthorizationMode = authorizationMode;
        Critical = critical;
    }
}
