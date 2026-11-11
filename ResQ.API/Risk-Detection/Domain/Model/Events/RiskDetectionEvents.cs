namespace ResQ.API.Risk_Detection.Domain.Model.Events;

/// <summary>
/// Domain event raised by the RiskDetection aggregate. It communicates the detection result;
/// Risk Detection does not define the alert, notification, actuator command or incident created downstream.
/// </summary>
public interface IRiskDetectionDomainEvent
{
    Guid RiskDetectionId { get; }

    DateTimeOffset OccurredAt { get; }
}

/// <summary>
/// Raised when a satisfied rule creates a new risk detection.
/// </summary>
public sealed record RiskDetectedEvent(
    Guid RiskDetectionId,
    Guid RuleId,
    string RiskType,
    string Severity,
    Guid? BuildingId,
    Guid? ZoneId,
    DateTimeOffset OccurredAt) : IRiskDetectionDomainEvent;

/// <summary>
/// Raised when the severity of an existing detection increases.
/// </summary>
public sealed record RiskSeverityEscalatedEvent(
    Guid RiskDetectionId,
    Guid RuleId,
    string RiskType,
    string PreviousSeverity,
    string NewSeverity,
    Guid? BuildingId,
    Guid? ZoneId,
    DateTimeOffset OccurredAt) : IRiskDetectionDomainEvent;
