using ResQ.API.Connectivity_Management.Domain.Model.Commands;
using ResQ.API.Connectivity_Management.Interfaces.REST.Resources;

namespace ResQ.API.Connectivity_Management.Interfaces.REST.Transform;

public static class RegisterHeartbeatCommandFromResourceAssembler
{
    /// <summary>
    /// The heartbeat timestamp is the reception time on the server.
    /// </summary>
    public static RegisterHeartbeatCommand ToCommandFromResource(Guid organizationId, RegisterHeartbeatResource resource)
    {
        if (resource.SensorId == Guid.Empty)
            throw new ArgumentException("Sensor id is required.");

        return new RegisterHeartbeatCommand(organizationId, resource.SensorId, DateTimeOffset.UtcNow, resource.SignalStrength);
    }
}
