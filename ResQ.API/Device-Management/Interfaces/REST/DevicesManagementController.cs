using Microsoft.AspNetCore.Mvc;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Domain.Services;
using ResQ.API.Device_Management.Interfaces.REST.Resources;
using ResQ.API.Device_Management.Interfaces.REST.Transform;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Device_Management.Interfaces.REST;

[ApiController]
[Route("api/v1/devices")]
[Produces("application/json")]
[Authorize]
public class DevicesManagementController(IDeviceCommandService deviceCommandService, 
    IDeviceQueryService deviceQueryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Registrar dispositivo", 
        Description = "Registra un nuevo dispositivo IoT dentro de la organización.", 
        OperationId = "RegisterDevice")]
    [SwaggerResponse(
        StatusCodes.Status201Created,
        "Dispositivo registrado correctamente",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest,
        "Datos inválidos")]
    [SwaggerResponse(
        StatusCodes.Status409Conflict,
        "El código o referencia externa ya se encuentra registrado")]
    public async Task<IActionResult> RegisterDevice(
        [FromBody] RegisterDeviceResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = RegisterDeviceCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);

            var device = await deviceCommandService.Handle(command);

            if (device == null) return BadRequest();

            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);

            AddETag(device.Version);

            return CreatedAtAction(nameof(GetDeviceById), new { deviceId = device.Id }, deviceResource);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar dispositivos",
        Description = "Obtiene los dispositivos de la organización aplicando filtros opcionales.",
        OperationId = "GetDevices")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Lista de dispositivos",
        typeof(DevicePageResource))]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest,
        "Parámetros de consulta inválidos")]
    public async Task<IActionResult> GetDevices(
        [FromQuery] Guid? buildingId,
        [FromQuery] Guid? zoneId,
        [FromQuery] EDeviceAdministrativeStatus? administrativeStatus,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (page < 0)
        {
            return BadRequest(new
            {
                message = "Page cannot be negative."
            });
        }

        if (size < 1 || size > 100)
        {
            return BadRequest(new
            {
                message = "Size must be between 1 and 100."
            });
        }

        var query =
            DeviceQueriesFromRequestAssembler
                .ToGetDevicesQuery(
                    organizationId,
                    buildingId,
                    zoneId,
                    administrativeStatus,
                    page,
                    size);

        var result = await deviceQueryService.Handle(query);

        var resource = DevicePageResourceFromPageAssembler.ToResourceFromPage(result);

        return Ok(resource);
    }

    [HttpGet("{deviceId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener dispositivo por ID",
        Description = "Obtiene los detalles de un dispositivo registrado.",
        OperationId = "GetDeviceById")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Dispositivo encontrado",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Dispositivo no encontrado")]
    public async Task<IActionResult> GetDeviceById(Guid deviceId)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var query = DeviceQueriesFromRequestAssembler.ToGetDeviceByIdQuery(organizationId, deviceId);

        var device = await deviceQueryService.Handle(query);

        if (device == null) return NotFound();

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);

        AddETag(device.Version);

        return Ok(resource);
    }

    [HttpGet("by-external-reference")]
    [SwaggerOperation(
        Summary = "Obtener dispositivo por referencia externa",
        Description = "Busca un dispositivo mediante su sistema de origen e identificador externo.",
        OperationId = "GetDeviceByExternalReference")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Dispositivo encontrado",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest,
        "Referencia externa inválida")]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Dispositivo no encontrado")]
    public async Task<IActionResult> GetDeviceByExternalReference(
        [FromQuery] string sourceSystem,
        [FromQuery] string externalDeviceId)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(sourceSystem) || string.IsNullOrWhiteSpace(externalDeviceId))
        {
            return BadRequest(new
            {
                message = "Source system and external device id are required."
            });
        }

        var query = DeviceQueriesFromRequestAssembler.ToGetDeviceByExternalReferenceQuery(organizationId, sourceSystem, externalDeviceId);

        var device = await deviceQueryService.Handle(query);

        if (device == null) return NotFound();

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);

        AddETag(device.Version);

        return Ok(resource);
    }

    [HttpPut("{deviceId:guid}/details")]
    [SwaggerOperation(
        Summary = "Actualizar detalles del dispositivo",
        Description = "Actualiza el nombre, descripción y especificaciones del dispositivo.",
        OperationId = "UpdateDeviceDetails")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Dispositivo actualizado",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Dispositivo no encontrado")]
    [SwaggerResponse(
        StatusCodes.Status409Conflict,
        "La operación no está permitida")]
    [SwaggerResponse(
        StatusCodes.Status412PreconditionFailed,
        "La versión del dispositivo no coincide")]
    [SwaggerResponse(
        StatusCodes.Status428PreconditionRequired,
        "Se requiere If-Match")]
    public async Task<IActionResult> UpdateDeviceDetails(
        Guid deviceId,
        [FromBody] UpdateDeviceDetailsResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var versionResult = GetExpectedVersion();

        if (versionResult.Error != null) return versionResult.Error;

        try
        {
            var command = UpdateDeviceDetailsCommandFromResourceAssembler
                    .ToCommandFromResource(
                        organizationId,
                        deviceId,
                        versionResult.Version,
                        resource);

            var device = await deviceCommandService.Handle(command);

            if (device == null) return NotFound();

            AddETag(device.Version);

            return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return HandleInvalidOperation(e);
        }
    }

    [HttpPut("{deviceId:guid}/capabilities")]
    [SwaggerOperation(
        Summary = "Reemplazar capacidades del dispositivo",
        Description = "Reemplaza las capacidades de un dispositivo inactivo.",
        OperationId = "ReplaceDeviceCapabilities")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Capacidades actualizadas",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Dispositivo no encontrado")]
    [SwaggerResponse(
        StatusCodes.Status409Conflict,
        "El dispositivo no permite modificar sus capacidades")]
    [SwaggerResponse(
        StatusCodes.Status412PreconditionFailed,
        "La versión del dispositivo no coincide")]
    [SwaggerResponse(
        StatusCodes.Status428PreconditionRequired,
        "Se requiere If-Match")]
    public async Task<IActionResult> ReplaceDeviceCapabilities(
        Guid deviceId,
        [FromBody] ReplaceDeviceCapabilitiesResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var versionResult = GetExpectedVersion();

        if (versionResult.Error != null) return versionResult.Error;

        try
        {
            var command = ReplaceDeviceCapabilitiesCommandFromResourceAssembler
                    .ToCommandFromResource(
                        organizationId,
                        deviceId,
                        versionResult.Version,
                        resource);

            var device = await deviceCommandService.Handle(command);

            if (device == null) return NotFound();

            AddETag(device.Version);

            return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return HandleInvalidOperation(e);
        }
    }

    [HttpPut("{deviceId:guid}/assignment")]
    [SwaggerOperation(
        Summary = "Asignar dispositivo a una ubicación",
        Description = "Asigna o reasigna un dispositivo inactivo a un edificio y opcionalmente a una zona.",
        OperationId = "AssignDeviceToLocation")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Ubicación actualizada",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Dispositivo no encontrado")]
    [SwaggerResponse(
        StatusCodes.Status409Conflict,
        "El dispositivo no permite modificar su ubicación")]
    [SwaggerResponse(
        StatusCodes.Status412PreconditionFailed,
        "La versión del dispositivo no coincide")]
    [SwaggerResponse(
        StatusCodes.Status428PreconditionRequired,
        "Se requiere If-Match")]
    public async Task<IActionResult> AssignDeviceToLocation(
        Guid deviceId,
        [FromBody] AssignDeviceToLocationResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var versionResult = GetExpectedVersion();

        if (versionResult.Error != null) return versionResult.Error;

        try
        {
            var command = AssignDeviceToLocationCommandFromResourceAssembler
                    .ToCommandFromResource(
                        organizationId,
                        deviceId,
                        versionResult.Version,
                        resource);

            var device = await deviceCommandService.Handle(command);

            if (device == null) return NotFound();

            AddETag(device.Version);

            return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return HandleInvalidOperation(e);
        }
    }

    [HttpPut("{deviceId:guid}/administrative-status")]
    [SwaggerOperation(
        Summary = "Cambiar estado administrativo",
        Description = "Cambia el estado administrativo del dispositivo.",
        OperationId = "ChangeDeviceAdministrativeStatus")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Estado administrativo actualizado",
        typeof(DeviceResource))]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Dispositivo no encontrado")]
    [SwaggerResponse(
        StatusCodes.Status409Conflict,
        "Transición de estado inválida")]
    [SwaggerResponse(
        StatusCodes.Status412PreconditionFailed,
        "La versión del dispositivo no coincide")]
    [SwaggerResponse(
        StatusCodes.Status428PreconditionRequired,
        "Se requiere If-Match")]
    public async Task<IActionResult> ChangeDeviceAdministrativeStatus(
        Guid deviceId,
        [FromBody] ChangeDeviceAdministrativeStatusResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var versionResult = GetExpectedVersion();

        if (versionResult.Error != null) return versionResult.Error;

        try
        {
            var command = ChangeDeviceAdministrativeStatusCommandFromResourceAssembler
                    .ToCommandFromResource(
                        organizationId,
                        deviceId,
                        versionResult.Version,
                        resource);

            var device = await deviceCommandService.Handle(command);

            if (device == null) return NotFound();

            AddETag(device.Version);

            return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException e)
        {
            return HandleInvalidOperation(e);
        }
    }

    private bool TryGetOrganizationId(out Guid organizationId)
    {
        organizationId = Guid.Empty;

        if (HttpContext.Items["OrganizationId"] is Guid id && id != Guid.Empty)
        {
            organizationId = id; 
            return true;
        }

        if (HttpContext.Items["OrganizationId"] is string value && Guid.TryParse(value, out id) && id != Guid.Empty)
        {
            organizationId = id;
            return true;
        }

        return false;
    }

    private (long Version, IActionResult? Error) GetExpectedVersion()
    {
        if (!Request.Headers.ContainsKey("If-Match"))
        {
            return (
                0,
                StatusCode(
                    StatusCodes.Status428PreconditionRequired,
                    new
                    {
                        message = "If-Match header is required."
                    }));
        }

        var value =
            Request.Headers["If-Match"]
                .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(value))
        {
            return (
                0,
                BadRequest(new
                {
                    message = "If-Match header is invalid."
                }));
        }

        value = value.Trim();

        if (value.StartsWith("W/"))
            value = value[2..].Trim();

        value = value.Trim('"');

        if (!long.TryParse(value, out var version))
        {
            return (
                0,
                BadRequest(new
                {
                    message = "If-Match header is invalid."
                }));
        }

        return (version, null);
    }

    private void AddETag(long version)
    {
        Response.Headers["ETag"] = $"\"{version}\"";
    }

    private IActionResult HandleInvalidOperation(
        InvalidOperationException exception)
    {
        if (exception.Message ==
            "The device was modified by another operation.")
        {
            return StatusCode(
                StatusCodes.Status412PreconditionFailed,
                new
                {
                    message = exception.Message
                });
        }

        return Conflict(new
        {
            message = exception.Message
        });
    }
}