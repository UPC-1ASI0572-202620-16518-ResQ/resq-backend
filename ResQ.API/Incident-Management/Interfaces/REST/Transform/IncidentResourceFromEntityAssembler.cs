using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Interfaces.REST.Resources;

namespace ResQ.API.Incident_Management.Interfaces.REST.Transform;

public static class IncidentResourceFromEntityAssembler
{
    public static IncidentResource ToResourceFromEntity(Incident entity)
    {
        return new IncidentResource(
            entity.Id.Value,
            entity.OrganizationId,
            entity.ZoneId.Value,
            entity.RiskDetectionId,
            entity.AlertId,
            entity.Type.ToString(),
            entity.Level.ToString(),
            entity.Status.ToString(),
            entity.AssignedTo?.Value,
            entity.AssignedUserId,
            entity.AssignedAt,
            entity.CreatedAt,
            entity.ResolvedAt,
            entity.ResolvedByUserId,
            entity.ClosedAt,
            entity.ClosedByUserId,
            entity.ResolutionNotes,
            entity.IsSafetyVerified,
            entity.HumanActions.OrderBy(action => action.PerformedAt).Select(action =>
                new IncidentHumanActionResource(action.Id, action.UserId, action.ActionCode, action.Notes, action.PerformedAt)),
            entity.SafetyVerifications.OrderBy(verification => verification.VerifiedAt).Select(verification =>
                new IncidentSafetyVerificationResource(
                    verification.Id, verification.VerifiedByUserId, verification.Safe, verification.Reason,
                    verification.DeviceId, verification.MeasurementId, verification.MeasuredValue,
                    verification.MeasuredAt, verification.RuleSnapshot, verification.VerifiedAt)));
    }
}
