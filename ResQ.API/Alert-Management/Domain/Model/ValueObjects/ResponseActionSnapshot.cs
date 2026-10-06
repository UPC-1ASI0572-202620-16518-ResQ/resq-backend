namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Response action requested for an alert: which actuator capability of which device must act,
/// and whether a human has to authorize it first.
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

    public ResponseActionSnapshot(string actionCode, Guid targetDeviceId, string targetCapabilityCode,
        EAuthorizationMode authorizationMode, bool critical)
    {
        if (string.IsNullOrWhiteSpace(actionCode))
            throw new ArgumentException("Action code is required.");

        if (actionCode.Trim().Length > 80)
            throw new ArgumentException("Action code cannot exceed 80 characters.");

        if (targetDeviceId == Guid.Empty)
            throw new ArgumentException("Target device id is required.");

        if (string.IsNullOrWhiteSpace(targetCapabilityCode))
            throw new ArgumentException("Target capability code is required.");

        if (targetCapabilityCode.Trim().Length > 80)
            throw new ArgumentException("Target capability code cannot exceed 80 characters.");

        ActionId = Guid.NewGuid();
        ActionCode = actionCode.Trim().ToUpperInvariant();
        TargetDeviceId = targetDeviceId;
        TargetCapabilityCode = targetCapabilityCode.Trim();
        AuthorizationMode = authorizationMode;
        Critical = critical;
    }
}
