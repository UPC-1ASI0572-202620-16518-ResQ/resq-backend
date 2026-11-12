using ResQ.API.Monitoring.Domain.Model.Commands;
using ResQ.API.Monitoring.Interfaces.REST.Resources;

namespace ResQ.API.Monitoring.Interfaces.REST.Transform;

public static class RecordMeasurementCommandFromResourceAssembler
{
    public static RecordMeasurementCommand ToCommandFromResource(
        Guid organizationId,
        RecordMeasurementResource resource)
    {
        if (resource.Value is null)
            throw new ArgumentException("Measurement value is required.", nameof(resource.Value));

        return new RecordMeasurementCommand(
            organizationId,
            resource.DeviceId,
            resource.VariableType,
            resource.Value.Value,
            resource.Unit,
            resource.MeasuredAt);
    }
}