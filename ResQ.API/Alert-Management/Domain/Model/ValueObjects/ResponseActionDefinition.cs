namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Data used to configure an action inside a response policy.
/// </summary>
public record ResponseActionDefinition(
    string ActionCode,
    Guid TargetDeviceId,
    string TargetCapabilityCode,
    EAuthorizationMode AuthorizationMode,
    bool Critical);
