namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Matches DecideResponseAuthorizationResourceDto in the frontend. Decision is APPROVED or REJECTED.
/// </summary>
public record DecideResponseAuthorizationResource(string Decision);
