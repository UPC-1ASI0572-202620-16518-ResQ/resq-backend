using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQ.API.Connectivity_Management.Domain.Model.Queries;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;
using ResQ.API.Connectivity_Management.Domain.Services;
using ResQ.API.Connectivity_Management.Interfaces.REST.Resources;
using ResQ.API.Connectivity_Management.Interfaces.REST.Transform;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Connectivity_Management.Interfaces.REST;

/// <summary>
/// Controlador REST de conectividad de sensores (Connectivity Management Bounded Context).
/// Recibe los latidos (heartbeats) de los dispositivos IoT y expone su estado de red.
/// </summary>
[ApiController]
[Route("api/v1/connectivity")]
[Produces("application/json")]
[Authorize]
public class ConnectivityController(
    IConnectivityCommandService connectivityCommandService,
    IConnectivityQueryService connectivityQueryService,
    IOptions<ConnectivitySettings> settings) : ControllerBase
{
    [HttpPost("heartbeats")]
    [SwaggerOperation(
        Summary = "Registrar latido (heartbeat)",
        Description = @"**Propósito:**
Registra una señal de vida de un sensor. Si es el primer latido se crea su conexión;
si estaba `OFFLINE` o `TIMEOUT`, vuelve a `ONLINE`.

**Integración con Device Management:** `sensorId` debe ser el id de un dispositivo registrado en la organización (si no, 404).

---
### JSON de Prueba
```json
{
  ""sensorId"": ""{deviceId}"",
  ""signalStrength"": -65
}
```
`signalStrength` es opcional, en dBm, entre -100 y 0.",
        OperationId = "RegisterHeartbeat")]
    [SwaggerResponse(StatusCodes.Status200OK, "Latido registrado", typeof(SensorConnectionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos (p. ej. señal fuera de rango)")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "El sensor no es un dispositivo registrado")]
    public async Task<IActionResult> RegisterHeartbeat([FromBody] RegisterHeartbeatResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command = RegisterHeartbeatCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);
            var connection = await connectivityCommandService.Handle(command);

            return Ok(SensorConnectionResourceFromEntityAssembler.ToResourceFromEntity(
                connection, DateTimeOffset.UtcNow, settings.Value.HeartbeatTimeout));
        });
    }

    [HttpGet("sensors/{sensorId:guid}/status")]
    [SwaggerOperation(
        Summary = "Obtener estado de red de un sensor",
        Description = @"Devuelve `ONLINE`, `OFFLINE` o `TIMEOUT`, la hora del último latido y la intensidad de señal.
Un sensor cuyo último latido supera `ConnectivitySettings:HeartbeatTimeoutSeconds` se reporta como `TIMEOUT`.
404 si el sensor nunca envió un latido.",
        OperationId = "GetSensorConnectionStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado del sensor", typeof(SensorConnectionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "El sensor no tiene conexión registrada")]
    public async Task<IActionResult> GetSensorConnectionStatus(Guid sensorId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var connection = await connectivityQueryService.Handle(new GetSensorConnectionStatusQuery(organizationId, sensorId));

        if (connection is null) return NotFound(new { message = "Sensor connection not found." });

        return Ok(SensorConnectionResourceFromEntityAssembler.ToResourceFromEntity(
            connection, DateTimeOffset.UtcNow, settings.Value.HeartbeatTimeout));
    }

    [HttpGet("sensors/offline")]
    [SwaggerOperation(
        Summary = "Listar sensores sin conexión",
        Description = "Lista los sensores de la organización en estado `OFFLINE` o `TIMEOUT`, del latido más antiguo al más reciente.",
        OperationId = "GetAllOfflineSensors")]
    [SwaggerResponse(StatusCodes.Status200OK, "Sensores sin conexión", typeof(IEnumerable<SensorConnectionResource>))]
    public async Task<IActionResult> GetAllOfflineSensors()
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var connections = await connectivityQueryService.Handle(new GetAllOfflineSensorsQuery(organizationId));
        var now = DateTimeOffset.UtcNow;

        return Ok(connections.Select(connection =>
            SensorConnectionResourceFromEntityAssembler.ToResourceFromEntity(connection, now, settings.Value.HeartbeatTimeout)));
    }
}
