namespace ResQ.API.Alert_Management.Interfaces.REST.Resources;

/// <summary>
/// Outcome reported by the actuator for a response execution.
/// </summary>
public record RecordResponseExecutionResultResource(bool Successful, string ResultCode, string? Message);
