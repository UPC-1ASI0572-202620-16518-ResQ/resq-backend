namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Decision registered by an operator for a response that requires human authorization.
/// </summary>
public enum EAuthorizationDecision
{
    Approved,
    Rejected
}
