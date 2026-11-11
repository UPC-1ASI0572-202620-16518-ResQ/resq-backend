using ResQ.API.Risk_Detection.Domain.Model.Commands;

namespace ResQ.API.Risk_Detection.Domain.Services;

public interface IRiskDetectionCommandService
{
    Task<MeasurementEvaluationResult> Handle(EvaluateMeasurementCommand command);
}
