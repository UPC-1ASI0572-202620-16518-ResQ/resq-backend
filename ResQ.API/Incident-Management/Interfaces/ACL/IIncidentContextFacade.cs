namespace ResQ.API.Incident_Management.Interfaces.ACL;

public interface IIncidentContextFacade
{
    Task<Guid> CreateIncident(Guid organizationId, Guid zoneId, string type, string level);
}