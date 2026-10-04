using System.Text.RegularExpressions;
using ResQ.API.Alert_Management.Domain.Model.Entities;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Alert_Management.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root that defines which actuator actions are requested when an alert
/// of a given risk type is generated.
/// </summary>
public partial class ResponsePolicy
{
    private readonly List<ResponseAction> _actions = new();

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Risk type that triggers the policy (e.g. GAS_LEAK).
    /// </summary>
    public string RiskTypeCode { get; private set; } = string.Empty;

    public EResponsePolicyStatus Status { get; private set; }

    public IReadOnlyCollection<ResponseAction> Actions => _actions.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Incremented on every change.
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    protected ResponsePolicy()
    {
    }

    /// <summary>
    /// Configures a new policy. Policies start inactive so they can be reviewed before activation.
    /// </summary>
    public static ResponsePolicy Configure(Guid organizationId, string riskTypeCode, IEnumerable<ResponseActionDefinition> actions)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.");

        ArgumentNullException.ThrowIfNull(actions);

        var definitions = NormalizeActions(actions);
        var now = DateTimeOffset.UtcNow;

        var policy = new ResponsePolicy
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            RiskTypeCode = RiskCodes.NormalizeRiskTypeCode(riskTypeCode),
            Status = EResponsePolicyStatus.Inactive,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1
        };

        foreach (var definition in definitions)
        {
            policy._actions.Add(new ResponseAction(Guid.NewGuid(), definition));
        }

        return policy;
    }

    /// <summary>
    /// Replaces the risk type and actions. Actions keep their identifier when the action code is unchanged.
    /// </summary>
    public void Update(string riskTypeCode, IEnumerable<ResponseActionDefinition> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var normalizedRiskType = RiskCodes.NormalizeRiskTypeCode(riskTypeCode);
        var definitions = NormalizeActions(actions);

        if (Status == EResponsePolicyStatus.Active && definitions.Count == 0)
            throw new InvalidOperationException("An active response policy must contain at least one valid action.");

        if (RiskTypeCode == normalizedRiskType && ActionsAreEquivalent(definitions))
            return;

        var current = _actions.ToDictionary(action => action.ActionCode, StringComparer.Ordinal);
        var next = new List<ResponseAction>();

        foreach (var definition in definitions)
        {
            if (current.TryGetValue(definition.ActionCode, out var existing))
            {
                existing.Update(definition);
                next.Add(existing);
            }
            else
            {
                next.Add(new ResponseAction(Guid.NewGuid(), definition));
            }
        }

        RiskTypeCode = normalizedRiskType;
        _actions.Clear();
        _actions.AddRange(next);

        RegisterChange();
    }

    /// <summary>
    /// Activates or deactivates the policy.
    /// </summary>
    public void ChangeStatus(bool active)
    {
        var target = active ? EResponsePolicyStatus.Active : EResponsePolicyStatus.Inactive;

        if (Status == target)
            return;

        if (active && _actions.Count == 0)
            throw new InvalidOperationException("A response policy must contain at least one valid action before it can be activated.");

        Status = target;

        RegisterChange();
    }

    public bool IsActive => Status == EResponsePolicyStatus.Active;

    private bool ActionsAreEquivalent(IReadOnlyCollection<ResponseActionDefinition> definitions)
    {
        return _actions.Count == definitions.Count &&
               definitions.All(definition => _actions.Any(action => action.IsEquivalentTo(definition)));
    }

    private static List<ResponseActionDefinition> NormalizeActions(IEnumerable<ResponseActionDefinition> actions)
    {
        var normalized = new List<ResponseActionDefinition>();

        foreach (var action in actions)
        {
            if (action is null)
                throw new ArgumentException("Response actions cannot be null.");

            if (string.IsNullOrWhiteSpace(action.ActionCode))
                throw new ArgumentException("Each response action requires an action code.");

            var actionCode = action.ActionCode.Trim().ToUpperInvariant();

            if (actionCode.Length > 80 || !ActionCodePattern().IsMatch(actionCode))
                throw new ArgumentException($"Action code '{action.ActionCode}' must use letters, digits and underscores, up to 80 characters.");

            if (action.TargetDeviceId == Guid.Empty)
                throw new ArgumentException("Each response action requires a target device.");

            if (string.IsNullOrWhiteSpace(action.TargetCapabilityCode))
                throw new ArgumentException("Each response action requires a target capability.");

            if (action.TargetCapabilityCode.Trim().Length > 80)
                throw new ArgumentException("Target capability code cannot exceed 80 characters.");

            if (normalized.Any(item => item.ActionCode == actionCode))
                throw new ArgumentException("Response action codes must be unique within the policy.");

            normalized.Add(action with
            {
                ActionCode = actionCode,
                TargetCapabilityCode = action.TargetCapabilityCode.Trim()
            });
        }

        return normalized;
    }

    private void RegisterChange()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$")]
    private static partial Regex ActionCodePattern();
}
