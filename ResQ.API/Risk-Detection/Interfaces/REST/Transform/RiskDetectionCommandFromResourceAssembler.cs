using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Model.ValueObjects;
using ResQ.API.Risk_Detection.Interfaces.REST.Resources;

namespace ResQ.API.Risk_Detection.Interfaces.REST.Transform;

public static class RiskDetectionCommandFromResourceAssembler
{
    public static ConfigureDetectionRuleCommand ToCommandFromResource(Guid organizationId, ConfigureDetectionRuleResource resource)
    {
        return new ConfigureDetectionRuleCommand(
            organizationId,
            resource.RiskTypeCode,
            resource.SeverityCode,
            resource.VariableType,
            EnumCode.Parse<EComparisonOperator>(resource.Operator, "operator"),
            resource.Threshold);
    }

    public static ChangeDetectionRuleStatusCommand ToCommandFromResource(Guid organizationId, Guid ruleId,
        ChangeDetectionRuleStatusResource resource)
    {
        if (resource.Active is null)
            throw new ArgumentException("'active' is required.");

        return new ChangeDetectionRuleStatusCommand(organizationId, ruleId, resource.Active.Value);
    }

    public static EvaluateMeasurementCommand ToCommandFromResource(Guid organizationId, EvaluateMeasurementResource resource)
    {
        if (resource.Value is null)
            throw new ArgumentException("'value' is required.");

        var measurementId = resource.MeasurementId is null || resource.MeasurementId == Guid.Empty
            ? Guid.NewGuid()
            : resource.MeasurementId.Value;

        return new EvaluateMeasurementCommand(
            organizationId,
            measurementId,
            resource.DeviceId,
            resource.VariableType,
            resource.Value.Value,
            resource.MeasuredAt);
    }
}
