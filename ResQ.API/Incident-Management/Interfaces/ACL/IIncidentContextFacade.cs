namespace ResQ.API.Incident_Management.Interfaces.ACL;

public interface IIncidentContextFacade
{
    Task<Guid> CreateIncident(Guid organizationId, Guid zoneId, string type, string level);

    Task<Guid> CreateIncidentForDetection(
        Guid organizationId, Guid zoneId, string type, string level, Guid riskDetectionId, Guid? alertId);
}
