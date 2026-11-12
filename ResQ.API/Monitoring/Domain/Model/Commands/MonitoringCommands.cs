namespace ResQ.API.Monitoring.Domain.Model.Commands;

/// <summary>
/// Records a Cloud monitoring measurement and propagates it to Risk Detection.
/// </summary>
public record RecordMeasurementCommand(
    Guid OrganizationId,
    Guid DeviceId,
    string VariableType,
    decimal Value,
    string Unit,
    DateTimeOffset MeasuredAt);