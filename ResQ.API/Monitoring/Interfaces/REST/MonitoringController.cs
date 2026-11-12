using Microsoft.AspNetCore.Mvc;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Monitoring.Domain.Model.Queries;
using ResQ.API.Monitoring.Domain.Services;
using ResQ.API.Monitoring.Interfaces.REST.Resources;
using ResQ.API.Monitoring.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Monitoring.Interfaces.REST;

/// <summary>
/// Cloud Monitoring REST API. Receives measurements, maintains monitoring state
/// and propagates valid measurements to Risk Detection.
/// </summary>
[ApiController]
[Route("api/v1/monitoring")]
[Produces("application/json")]
[Authorize]
public class MonitoringController(
    IMonitoringCommandService monitoringCommandService,
    IMonitoringQueryService monitoringQueryService) : ControllerBase
{
    [HttpPost("measurements")]
    [SwaggerOperation(
        Summary = "Registrar una medición",
        Description = @"**Propósito:**
Registra una medición Cloud de un dispositivo activo, conserva su ubicación al momento de la medición,
actualiza el estado de Monitoring y encadena la medición hacia Risk Detection.

La medición debe corresponder a una capacidad de tipo `measurement` del dispositivo.

---
### JSON de Prueba
```json
{
  ""deviceId"": ""{deviceId}"",
  ""variableType"": ""GAS_PPM"",
  ""value"": 750,
  ""unit"": ""PPM"",
  ""measuredAt"": ""2026-11-11T15:00:00Z""
}
```

`measuredAt` puede ser histórico, pero no puede estar más de 5 minutos en el futuro.",
        OperationId = "RecordMonitoringMeasurement")]
    [SwaggerResponse(StatusCodes.Status201Created, "Medición registrada y evaluada", typeof(MeasurementIngestionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos de medición inválidos")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "El dispositivo no está activo")]
    public async Task<IActionResult> RecordMeasurement(
        [FromBody] RecordMeasurementResource resource)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId))
            return Unauthorized();

        return await this.ExecuteAsync(async () =>
        {
            var command =
                RecordMeasurementCommandFromResourceAssembler
                    .ToCommandFromResource(organizationId, resource);

            var result =
                await monitoringCommandService.Handle(command);

            var measurement =
                MonitoringResourceAssembler
                    .ToResourceFromEntity(result.Measurement);

            return StatusCode(
                StatusCodes.Status201Created,
                new MeasurementIngestionResource(
                    measurement.MeasurementId,
                    measurement.DeviceId,
                    measurement.BuildingId,
                    measurement.ZoneId,
                    measurement.VariableType,
                    measurement.MeasurementValue,
                    measurement.MeasuredAt,
                    measurement.RecordedAt,
                    result.RiskDetectionIds));
        });
    }

    [HttpGet("buildings/{buildingId:guid}/status")]
    [SwaggerOperation(
        Summary = "Obtener estado de monitoreo de una edificación",
        Description =
            "Devuelve las zonas con estado de Monitoring y el conteo de dispositivos disponibles, no disponibles y desconocidos.",
        OperationId = "GetBuildingMonitoringStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado de la edificación", typeof(BuildingMonitoringStatusResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No existe estado de Monitoring para la edificación")]
    public async Task<IActionResult> GetBuildingMonitoringStatus(
        Guid buildingId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId))
            return Unauthorized();

        var snapshot =
            await monitoringQueryService.Handle(
                new GetBuildingMonitoringStatusQuery(
                    organizationId,
                    buildingId));

        return snapshot is null
            ? NotFound(new
            {
                message = "Building monitoring state not found."
            })
            : Ok(
                MonitoringResourceAssembler
                    .ToResourceFromSnapshot(snapshot));
    }

    [HttpGet("zones/{zoneId:guid}/status")]
    [SwaggerOperation(
        Summary = "Obtener estado de monitoreo de una zona",
        Description =
            "Devuelve la última observación de la zona, el riesgo activo si existe y la condición actual.",
        OperationId = "GetZoneMonitoringStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado de la zona", typeof(ZoneMonitoringStateResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No existe estado de Monitoring para la zona")]
    public async Task<IActionResult> GetZoneMonitoringStatus(
        Guid zoneId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId))
            return Unauthorized();

        var state =
            await monitoringQueryService.Handle(
                new GetZoneMonitoringStatusQuery(
                    organizationId,
                    zoneId));

        return state is null
            ? NotFound(new
            {
                message = "Zone monitoring state not found."
            })
            : Ok(
                MonitoringResourceAssembler
                    .ToResourceFromEntity(state));
    }

    [HttpGet("devices/{deviceId:guid}/status")]
    [SwaggerOperation(
        Summary = "Obtener estado de monitoreo de un dispositivo",
        Description =
            "Devuelve la disponibilidad derivada de Connectivity y la hora de la última medición recibida.",
        OperationId = "GetDeviceMonitoringStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado del dispositivo", typeof(DeviceMonitoringStateResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No existe estado de Monitoring para el dispositivo")]
    public async Task<IActionResult> GetDeviceMonitoringStatus(
        Guid deviceId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId))
            return Unauthorized();

        var state =
            await monitoringQueryService.Handle(
                new GetDeviceMonitoringStatusQuery(
                    organizationId,
                    deviceId));

        if (state is null)
        {
            return NotFound(new
            {
                message = "Device monitoring state not found."
            });
        }

        var availability =
            await monitoringQueryService
                .GetEffectiveDeviceAvailabilityAsync(
                    organizationId,
                    deviceId);

        return Ok(
            MonitoringResourceAssembler
                .ToResourceFromEntity(
                    state,
                    availability));
    }

    [HttpGet("devices/{deviceId:guid}/measurements/current")]
    [SwaggerOperation(
        Summary = "Obtener mediciones vigentes de un dispositivo",
        Description =
            "Devuelve únicamente la medición más reciente de cada variable que se encuentre dentro de MonitoringSettings:MeasurementMaxAgeSeconds.",
        OperationId = "GetCurrentDeviceMeasurements")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mediciones vigentes", typeof(IEnumerable<MonitoringMeasurementResource>))]
    public async Task<IActionResult> GetCurrentDeviceMeasurements(
        Guid deviceId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId))
            return Unauthorized();

        var measurements =
            await monitoringQueryService.Handle(
                new GetCurrentDeviceMeasurementsQuery(
                    organizationId,
                    deviceId,
                    DateTimeOffset.UtcNow));

        return Ok(
            measurements.Select(
                measurement =>
                    MonitoringResourceAssembler
                        .ToResourceFromEntity(measurement)));
    }

    [HttpGet("zones/{zoneId:guid}/measurements/current")]
    [SwaggerOperation(
        Summary = "Obtener mediciones vigentes de una zona",
        Description =
            "Devuelve únicamente las mediciones vigentes de la zona, agrupadas por dispositivo y variable.",
        OperationId = "GetCurrentZoneMeasurements")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mediciones vigentes", typeof(IEnumerable<MonitoringMeasurementResource>))]
    public async Task<IActionResult> GetCurrentZoneMeasurements(
        Guid zoneId)
    {
        if (!HttpContext.TryGetOrganizationId(out var organizationId))
            return Unauthorized();

        var measurements =
            await monitoringQueryService.Handle(
                new GetCurrentZoneMeasurementsQuery(organizationId, zoneId, DateTimeOffset.UtcNow));

        return Ok(measurements.Select(measurement => MonitoringResourceAssembler.ToResourceFromEntity(measurement)));
    }
}