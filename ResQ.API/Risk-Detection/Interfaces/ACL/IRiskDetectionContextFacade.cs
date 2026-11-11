namespace ResQ.API.Risk_Detection.Interfaces.ACL;

/// <summary>
/// Anti-Corruption Layer (ACL) interface exposed to other bounded contexts.
/// Monitoring will call it for every valid measurement it receives.
/// </summary>
public interface IRiskDetectionContextFacade
{
    /// <summary>
    /// Evaluates a valid measurement against the active detection rules.
    /// </summary>
    /// <returns>Identifiers of the risk detections created or updated (empty when no rule was satisfied).</returns>
    /// <exception cref="ArgumentException">When the measurement is not valid.</exception>
    Task<IReadOnlyCollection<Guid>> EvaluateMeasurementAsync(
        Guid organizationId,
        Guid measurementId,
        Guid deviceId,
        string variableType,
        decimal value,
        DateTimeOffset measuredAt);
}
