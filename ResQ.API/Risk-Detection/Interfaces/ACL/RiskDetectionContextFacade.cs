using ResQ.API.Risk_Detection.Domain.Model.Commands;
using ResQ.API.Risk_Detection.Domain.Services;

namespace ResQ.API.Risk_Detection.Interfaces.ACL;

/// <summary>
/// Implementation of the Anti-Corruption Layer facade for Risk Detection.
/// </summary>
public class RiskDetectionContextFacade(IRiskDetectionCommandService riskDetectionCommandService) : IRiskDetectionContextFacade
{
    public async Task<IReadOnlyCollection<Guid>> EvaluateMeasurementAsync(
        Guid organizationId,
        Guid measurementId,
        Guid deviceId,
        string variableType,
        decimal value,
        DateTimeOffset measuredAt)
    {
        var command = new EvaluateMeasurementCommand(organizationId, measurementId, deviceId, variableType, value, measuredAt);
        var result = await riskDetectionCommandService.Handle(command);

        return result.RiskDetections.Select(detection => detection.Id).ToList();
    }
}
