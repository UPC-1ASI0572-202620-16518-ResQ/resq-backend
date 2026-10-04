using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Entities;

/// <summary>
/// Action that a response policy requests on an actuator capability. Belongs to the ResponsePolicy aggregate.
/// </summary>
public class ResponseAction
{
    public Guid Id { get; private set; }

    public string ActionCode { get; private set; } = string.Empty;

    public Guid TargetDeviceId { get; private set; }

    public string TargetCapabilityCode { get; private set; } = string.Empty;

    public EAuthorizationMode AuthorizationMode { get; private set; }

    public bool Critical { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected ResponseAction()
    {
    }

    internal ResponseAction(Guid id, ResponseActionDefinition definition)
    {
        Id = id;
        Apply(definition);
    }

    internal void Update(ResponseActionDefinition definition)
    {
        Apply(definition);
    }

    /// <summary>
    /// Copies the action as an immutable snapshot for a response execution.
    /// </summary>
    public ResponseActionSnapshot ToSnapshot()
    {
        return new ResponseActionSnapshot(Id, ActionCode, TargetDeviceId, TargetCapabilityCode, AuthorizationMode, Critical);
    }

    internal bool IsEquivalentTo(ResponseActionDefinition definition)
    {
        return ActionCode == definition.ActionCode &&
               TargetDeviceId == definition.TargetDeviceId &&
               TargetCapabilityCode == definition.TargetCapabilityCode &&
               AuthorizationMode == definition.AuthorizationMode &&
               Critical == definition.Critical;
    }

    private void Apply(ResponseActionDefinition definition)
    {
        ActionCode = definition.ActionCode;
        TargetDeviceId = definition.TargetDeviceId;
        TargetCapabilityCode = definition.TargetCapabilityCode;
        AuthorizationMode = definition.AuthorizationMode;
        Critical = definition.Critical;
    }
}
