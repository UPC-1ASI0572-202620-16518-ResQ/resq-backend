namespace ResQ.API.Alert_Management.Domain.Model.ValueObjects;

/// <summary>
/// Outcome reported by the actuator for a response execution.
/// </summary>
public record ExecutionResult
{
    public bool Successful { get; init; }

    public string ResultCode { get; init; } = string.Empty;

    public string? Message { get; init; }

    public DateTimeOffset CompletedAt { get; init; }

    /// <summary>
    /// Required by Entity Framework Core.
    /// </summary>
    private ExecutionResult()
    {
    }

    public ExecutionResult(bool successful, string resultCode, string? message)
    {
        if (string.IsNullOrWhiteSpace(resultCode))
            throw new ArgumentException("Result code is required.");

        if (resultCode.Trim().Length > 80)
            throw new ArgumentException("Result code cannot exceed 80 characters.");

        if (message?.Trim().Length > 500)
            throw new ArgumentException("Result message cannot exceed 500 characters.");

        Successful = successful;
        ResultCode = resultCode.Trim().ToUpperInvariant();
        Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
