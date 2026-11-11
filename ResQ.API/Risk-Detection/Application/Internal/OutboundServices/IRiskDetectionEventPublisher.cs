using ResQ.API.Risk_Detection.Domain.Model.Aggregates;

namespace ResQ.API.Risk_Detection.Application.Internal.OutboundServices;

/// <summary>
/// Publishes the domain events of the evaluated detections to the next stage of the ResQ flow,
/// without coupling the application layer to a specific downstream context.
/// </summary>
public interface IRiskDetectionEventPublisher
{
    /// <returns>Identifiers of the alerts generated downstream.</returns>
    Task<IReadOnlyCollection<Guid>> PublishAsync(Guid organizationId, IEnumerable<RiskDetection> detections);
}
