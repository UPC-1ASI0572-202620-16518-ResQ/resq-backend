using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;

namespace ResQ.API.Risk_Detection.Application.Internal.OutboundServices;

/// <summary>
/// Resolves the Building and Zone of the device that produced a measurement.
/// </summary>
public interface IRiskLocationResolver
{
    /// <returns>A RESOLVED location, or UNRESOLVED when the device context cannot determine it.</returns>
    Task<RiskLocation> ResolveAsync(Guid organizationId, Guid deviceId);
}
