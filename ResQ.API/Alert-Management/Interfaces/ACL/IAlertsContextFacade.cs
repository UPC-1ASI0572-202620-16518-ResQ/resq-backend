namespace ResQ.API.Alert_Management.Interfaces.ACL;

/// <summary>
/// Anti-Corruption Layer (ACL) interface exposed to other bounded contexts.
/// Risk Detection will use it to raise an alert when it confirms a risk.
/// </summary>
public interface IAlertsContextFacade
{
    /// <summary>
    /// Generates an alert for a confirmed risk detection and requests the actions of the active response policy.
    /// </summary>
    /// <returns>The identifier of the generated alert.</returns>
    Task<Guid> GenerateAlertAsync(
        Guid organizationId,
        string riskDetectionId,
        string riskTypeCode,
        string severityCode,
        Guid? buildingId,
        Guid? zoneId,
        DateTimeOffset detectedAt,
        IEnumerable<AlertRecipient> recipients);
}

/// <summary>
/// Recipient data accepted by the Alert Management ACL.
/// </summary>
public record AlertRecipient(string RecipientUserId, string Channel, string Destination);
