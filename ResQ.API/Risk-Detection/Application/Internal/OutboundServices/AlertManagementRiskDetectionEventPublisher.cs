using ResQ.API.Alert_Management.Interfaces.ACL;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Events;
using ResQ.API.Incident_Management.Interfaces.ACL;

namespace ResQ.API.Risk_Detection.Application.Internal.OutboundServices;

/// <summary>
/// Delivers RiskDetectedEvent and RiskSeverityEscalatedEvent to Alert &amp; Response through its ACL.
/// Risk Detection does not build the alert itself: Alert Management decides how to warn.
/// Adding evidence to an existing detection does not raise a new alert.
/// </summary>
public class AlertManagementRiskDetectionEventPublisher(
    IAlertsContextFacade alertsContextFacade,
    IIncidentContextFacade incidentContextFacade,
    ILogger<AlertManagementRiskDetectionEventPublisher> logger) : IRiskDetectionEventPublisher
{
    public async Task<RiskPublicationResult> PublishAsync(Guid organizationId, IEnumerable<RiskDetection> detections)
    {
        var alertIds = new List<Guid>();
        var incidentIds = new List<Guid>();

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

                    if (string.Equals(severity, "CRITICAL", StringComparison.OrdinalIgnoreCase) &&
                        detection.Location.ZoneId.HasValue)
                    {
                        var incidentId = await incidentContextFacade.CreateIncidentForDetection(
                            organizationId,
                            detection.Location.ZoneId.Value,
                            detection.RiskTypeCode,
                            severity,
                            detection.Id,
                            alertId == Guid.Empty ? null : alertId);
                        if (incidentId != Guid.Empty && !incidentIds.Contains(incidentId)) incidentIds.Add(incidentId);
                    }
                }
                catch (Exception e) when (e is ArgumentException or InvalidOperationException)
                {
                    // The detection is already persisted; a rejected alert must not undo it
                    logger.LogWarning(e, "Alert Management rejected the alert for risk detection {RiskDetectionId}.", detection.Id);
                }
            }
        }

        return new RiskPublicationResult(alertIds, incidentIds);
    }
}
