using ResQ.API.Alert_Management.Interfaces.ACL;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Events;

namespace ResQ.API.Risk_Detection.Application.Internal.OutboundServices;

/// <summary>
/// Delivers RiskDetectedEvent and RiskSeverityEscalatedEvent to Alert &amp; Response through its ACL.
/// Risk Detection does not build the alert itself: Alert Management decides how to warn.
/// Adding evidence to an existing detection does not raise a new alert.
/// </summary>
public class AlertManagementRiskDetectionEventPublisher(
    IAlertsContextFacade alertsContextFacade,
    ILogger<AlertManagementRiskDetectionEventPublisher> logger) : IRiskDetectionEventPublisher
{
    public async Task<IReadOnlyCollection<Guid>> PublishAsync(Guid organizationId, IEnumerable<RiskDetection> detections)
    {
        var alertIds = new List<Guid>();

        foreach (var detection in detections)
        {
            foreach (var domainEvent in detection.DomainEvents)
            {
                var (severity, detectedAt) = domainEvent switch
                {
                    RiskDetectedEvent detected => (detected.Severity, detection.DetectedAt),
                    RiskSeverityEscalatedEvent escalated => (escalated.NewSeverity, escalated.OccurredAt),
                    _ => (null, default)
                };

                if (severity is null) continue;

                try
                {
                    var alertId = await alertsContextFacade.GenerateAlertAsync(
                        organizationId,
                        detection.Id.ToString(),
                        detection.RiskTypeCode,
                        severity,
                        detection.Location.BuildingId,
                        detection.Location.ZoneId,
                        detectedAt,
                        []);

                    if (alertId != Guid.Empty) alertIds.Add(alertId);
                }
                catch (Exception e) when (e is ArgumentException or InvalidOperationException)
                {
                    // The detection is already persisted; a rejected alert must not undo it
                    logger.LogWarning(e, "Alert Management rejected the alert for risk detection {RiskDetectionId}.", detection.Id);
                }
            }
        }

        return alertIds;
    }
}
