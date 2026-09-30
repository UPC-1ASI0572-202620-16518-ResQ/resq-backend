namespace ResQ.API.Incident_Management.Interfaces.ACL;

public interface IIncidentContextFacade
{
    Task<Guid> CreateIncident(Guid zoneId, string type, string level);
}