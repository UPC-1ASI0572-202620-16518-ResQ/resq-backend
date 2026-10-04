namespace ResQ.API.Alert_Management.Domain.Model.Commands;

public record RecordResponseExecutionResultCommand(
    Guid OrganizationId,
    Guid ResponseExecutionId,
    bool Successful,
    string ResultCode,
    string? Message);
