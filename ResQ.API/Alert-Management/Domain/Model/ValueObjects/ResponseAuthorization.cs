namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Human decision registered for a response execution.
/// </summary>
public record ResponseAuthorization
{
    public Guid AuthorizationId { get; init; }

    public EAuthorizationDecision Decision { get; init; }

    public string DecidedByUserId { get; init; } = string.Empty;

    public DateTimeOffset DecidedAt { get; init; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    private ResponseAuthorization()
    {
    }

    public ResponseAuthorization(EAuthorizationDecision decision, string decidedByUserId)
    {
        if (string.IsNullOrWhiteSpace(decidedByUserId))
            throw new ArgumentException("The user who decides the authorization is required.");

        AuthorizationId = Guid.NewGuid();
        Decision = decision;
        DecidedByUserId = decidedByUserId.Trim();
        DecidedAt = DateTimeOffset.UtcNow;
    }
}
