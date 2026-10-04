namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Response policy returned by the API. Matches ResponsePolicyResourceDto in the frontend.
/// Status is ACTIVE or INACTIVE.
/// </summary>
public record ResponsePolicyResource(
    Guid PolicyId,
    Guid OrganizationId,
    string RiskTypeCode,
    string Status,
    IEnumerable<ResponseActionResource> Actions,
    long Version);

/// <summary>
/// Matches ResponseActionResourceDto in the frontend.
/// </summary>
public record ResponseActionResource(
    Guid ActionId,
    string ActionCode,
    Guid TargetDeviceId,
    string TargetCapabilityCode,
    string AuthorizationMode,
    bool Critical);

/// <summary>
/// Matches ResponseActionDataResourceDto in the frontend. AuthorizationMode is AUTOMATIC or HUMAN_REQUIRED.
/// </summary>
public record ResponseActionDataResource(
    string ActionCode,
    Guid TargetDeviceId,
    string TargetCapabilityCode,
    string AuthorizationMode,
    bool Critical);

/// <summary>
/// Matches ConfigureResponsePolicyResourceDto in the frontend.
/// </summary>
public record ConfigureResponsePolicyResource(string RiskTypeCode, IEnumerable<ResponseActionDataResource>? Actions);

/// <summary>
/// Matches UpdateResponsePolicyResourceDto in the frontend.
/// </summary>
public record UpdateResponsePolicyResource(string RiskTypeCode, IEnumerable<ResponseActionDataResource>? Actions);

/// <summary>
/// Matches ChangeResponsePolicyStatusResourceDto in the frontend.
/// </summary>
public record ChangeResponsePolicyStatusResource(bool Active);
