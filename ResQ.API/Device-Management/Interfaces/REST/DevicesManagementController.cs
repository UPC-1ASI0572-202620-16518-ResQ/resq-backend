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
public class DevicesManagementController(
    IDeviceCommandService deviceCommandService,
    IDeviceQueryService deviceQueryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Registrar dispositivo",
        Description = @"**Propósito:**
Registra un nuevo dispositivo IoT dentro de la organización.
**Integración con Building Management:** El dispositivo debe ser asignado obligatoriamente a una edificación activa (buildingId) y opcionalmente a una zona activa (zoneId) existentes en Building Management. La asignación es validada a través de BuildingsContextFacade.

---
### Encabezados Requeridos (Headers)
* Authorization: Bearer {token_jwt}
* Content-Type: application/json

---
### JSON de Prueba VÁLIDO (Copiar y Pegar en Swagger)
*Nota: Reemplaza buildingId y zoneId por los IDs de una edificación y zona reales previamente creadas en Building Management:*
```json
{
  ""deviceCode"": ""DEV-SMOKE-001"",
  ""name"": ""Sensor de Humo Óptico"",
  ""description"": ""Sensor fotoeléctrico de partículas de humo conectado por LoRaWAN"",
  ""specifications"": {
    ""manufacturer"": ""Bosch Security Systems"",
    ""model"": ""FCP-O320"",
    ""serialNumber"": ""SN-BOSCH-2026-001""
  },
  ""assignment"": {
    ""buildingId"": ""9c7299c6-5154-4fea-afc5-8e58e0771819"",
    ""zoneId"": ""84b48401-0d05-4abc-a65d-95ae151d1f38""
  },
  ""externalReference"": {
    ""sourceSystem"": ""AWS-IOT-CORE"",
    ""externalDeviceId"": ""urn:resq:device:smoke:001""
  },
  ""capabilities"": [
    {
      ""code"": ""SMOKE_DENSITY"",
      ""kind"": ""measurement"",
      ""unit"": ""PPM""
    },
    {
      ""code"": ""ALARM_BUZZER"",
      ""kind"": ""actuation"",
      ""unit"": null
    }
  ]
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones del sistema)
```json
{
  ""deviceCode"": ""dev smoke!"",
  ""name"": """",
  ""description"": ""Sensor con datos erróneos"",
  ""specifications"": {
    ""manufacturer"": """",
    ""model"": """",
    ""serialNumber"": """"
  },
  ""assignment"": {
    ""buildingId"": ""00000000-0000-0000-0000-000000000000"",
    ""zoneId"": null
  },
  ""externalReference"": null,
  ""capabilities"": []
}
```
**¿Por qué este JSON será RECHAZADO?**
1. deviceCode: contiene minúsculas y caracteres no permitidos (400 Bad Request).
2. name: está vacío (400 Bad Request).
3. assignment.buildingId: edificación inexistente o inactiva (409 Conflict vía BuildingsContextFacade).

---
### Respuesta Exitosa (201 Created)
Retorna el DeviceResource con estado inicial inactive.",
        OperationId = "RegisterDevice")]
    [SwaggerResponse(StatusCodes.Status201Created, "Dispositivo registrado correctamente", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Código duplicado, referencia externa duplicada o ubicación inválida")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = RegisterDeviceCommandFromResourceAssembler.ToCommandFromResource(organizationId, resource);
            var device = await deviceCommandService.Handle(command);

            if (device == null) return BadRequest();

            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
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
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de dispositivos", typeof(DevicePageResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parámetros de consulta inválidos")]
    public async Task<IActionResult> GetDevices(
        [FromQuery] Guid? buildingId,
        [FromQuery] Guid? zoneId,
        [FromQuery] EDeviceAdministrativeStatus? administrativeStatus,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (page < 0) return BadRequest(new { message = "Page cannot be negative." });
        if (size is < 1 or > 100) return BadRequest(new { message = "Size must be between 1 and 100." });

        var query = DeviceQueriesFromRequestAssembler.ToGetDevicesQuery(
            organizationId, buildingId, zoneId, administrativeStatus, page, size);

        var result = await deviceQueryService.Handle(query);
        var resource = DevicePageResourceFromPageAssembler.ToResourceFromPage(result);

        return Ok(resource);
    }

    [HttpGet("{deviceId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener dispositivo por ID",
        Description = "Obtiene los detalles de un dispositivo registrado.",
        OperationId = "GetDeviceById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Dispositivo encontrado", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    public async Task<IActionResult> GetDeviceById(Guid deviceId)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var query = DeviceQueriesFromRequestAssembler.ToGetDeviceByIdQuery(organizationId, deviceId);
        var device = await deviceQueryService.Handle(query);

        if (device == null) return NotFound();

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return Ok(resource);
    }

    [HttpGet("by-external-reference")]
    [SwaggerOperation(
        Summary = "Obtener dispositivo por referencia externa",
        Description = "Busca un dispositivo mediante su sistema de origen e identificador externo.",
        OperationId = "GetDeviceByExternalReference")]
    [SwaggerResponse(StatusCodes.Status200OK, "Dispositivo encontrado", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Referencia externa inválida")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    public async Task<IActionResult> GetDeviceByExternalReference(
        [FromQuery] string sourceSystem,
        [FromQuery] string externalDeviceId)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(sourceSystem) || string.IsNullOrWhiteSpace(externalDeviceId))
        {
            return BadRequest(new { message = "Source system and external device id are required." });
        }

        var query = DeviceQueriesFromRequestAssembler.ToGetDeviceByExternalReferenceQuery(organizationId, sourceSystem, externalDeviceId);
        var device = await deviceQueryService.Handle(query);

        if (device == null) return NotFound();

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return Ok(resource);
    }

    [HttpPut("{deviceId:guid}/details")]
    [SwaggerOperation(
        Summary = "Actualizar detalles del dispositivo",
        Description = @"**Propósito:**
Actualiza el nombre, descripción y/o especificaciones del dispositivo directamente.

**Actualización Parcial Soportada:**
Puedes enviar únicamente los campos que deseas modificar o dejar los demás en blanco, null o con 'string'. El sistema los ignorará y conservará los valores actuales del dispositivo.

---
### Encabezados Requeridos (Headers)
* Authorization: Bearer {token_jwt}
* Content-Type: application/json

---
### JSON de Prueba VÁLIDO - Cambiar solo el nombre (Copiar y Pegar en Swagger)
```json
{
  ""name"": ""Sensor de Humo Óptico - Piso 2""
}
```

---
### JSON de Prueba VÁLIDO - Con plantilla Swagger
```json
{
  ""name"": ""Sensor de Humo Óptico - Piso 2"",
  ""description"": ""string"",
  ""specifications"": {
    ""manufacturer"": ""string"",
    ""model"": ""string"",
    ""serialNumber"": ""string""
  }
}
```

---
### Respuesta Exitosa (200 OK)
Retorna el DeviceResource actualizado.",
        OperationId = "UpdateDeviceDetails")]
    [SwaggerResponse(StatusCodes.Status200OK, "Dispositivo actualizado", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La operación no está permitida")]
    public async Task<IActionResult> UpdateDeviceDetails(
        Guid deviceId,
        [FromBody] UpdateDeviceDetailsResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = UpdateDeviceDetailsCommandFromResourceAssembler.ToCommandFromResource(
                organizationId, deviceId, resource);

            var device = await deviceCommandService.Handle(command);
            if (device == null) return NotFound();

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
        Description = @"**Propósito:**
Reemplaza las capacidades de un dispositivo en estado Inactive.

---
### Encabezados Requeridos (Headers)
* Authorization: Bearer {token_jwt}
* Content-Type: application/json

---
### JSON de Prueba VÁLIDO (Copiar y Pegar en Swagger)
```json
{
  ""capabilities"": [
    {
      ""code"": ""SMOKE_DENSITY"",
      ""kind"": ""measurement"",
      ""unit"": ""PPM""
    },
    {
      ""code"": ""TEMPERATURE"",
      ""kind"": ""measurement"",
      ""unit"": ""CELSIUS""
    },
    {
      ""code"": ""ALARM_BUZZER"",
      ""kind"": ""actuation"",
      ""unit"": null
    }
  ]
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones)
```json
{
  ""capabilities"": [
    {
      ""code"": ""ACTUATOR_ERROR"",
      ""kind"": ""actuation"",
      ""unit"": ""VOLTS""
    }
  ]
}
```
*Nota: Las capacidades de tipo 'actuation' no pueden definir unidad de medida (unit debe ser null).*

---
### Respuesta Exitosa (200 OK)
Retorna el DeviceResource con las capacidades actualizadas.",
        OperationId = "ReplaceDeviceCapabilities")]
    [SwaggerResponse(StatusCodes.Status200OK, "Capacidades actualizadas", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Capacidades con formato inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "El dispositivo no permite modificar sus capacidades")]
    public async Task<IActionResult> ReplaceDeviceCapabilities(
        Guid deviceId,
        [FromBody] ReplaceDeviceCapabilitiesResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = ReplaceDeviceCapabilitiesCommandFromResourceAssembler.ToCommandFromResource(
                organizationId, deviceId, resource);

            var device = await deviceCommandService.Handle(command);
            if (device == null) return NotFound();

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
        Summary = "Asignar / Reasignar dispositivo a una ubicación",
        Description = @"**Propósito:**
Asigna o reubica un dispositivo inactivo a una edificación (buildingId) y opcionalmente a una zona (zoneId).
**Integración con Building Management:** Valida a través de BuildingsContextFacade que la edificación y la zona existan en la organización y se encuentren en estado Active.

---
### Encabezados Requeridos (Headers)
* Authorization: Bearer {token_jwt}
* Content-Type: application/json

---
### JSON de Prueba VÁLIDO (Copiar y Pegar en Swagger)
*Coloca los GUIDs de la edificación y zona reales creados en Building Management:*
```json
{
  ""buildingId"": ""9c7299c6-5154-4fea-afc5-8e58e0771819"",
  ""zoneId"": ""84b48401-0d05-4abc-a65d-95ae151d1f38""
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones de ubicación)
```json
{
  ""buildingId"": ""00000000-0000-0000-0000-000000000000"",
  ""zoneId"": null
}
```
**¿Por qué este JSON será RECHAZADO (409 Conflict)?**
BuildingsContextFacade verifica la existencia y estado activo de la edificación. Si el edificio no existe o está inactivo, rechaza la operación con 'The specified building or zone does not exist or is not active for device assignment.'.

---
### Reglas de Estado del Dispositivo
* Solo se permite reasignar ubicación cuando el dispositivo está en estado Inactive. Si está en otro estado, retornará 409 Conflict.

---
### Respuesta Exitosa (200 OK)
Retorna el DeviceResource con la nueva ubicación.",
        OperationId = "AssignDeviceToLocation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ubicación actualizada correctamente", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en la solicitud")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "El dispositivo no permite modificar su ubicación o la edificación/zona es inválida")]
    public async Task<IActionResult> AssignDeviceToLocation(
        Guid deviceId,
        [FromBody] AssignDeviceToLocationResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = AssignDeviceToLocationCommandFromResourceAssembler.ToCommandFromResource(
                organizationId, deviceId, resource);

            var device = await deviceCommandService.Handle(command);
            if (device == null) return NotFound();

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
        Description = @"**Propósito:**
Cambia el estado administrativo del dispositivo (inactive, active, retired). Para activarlo, su edificación y zona deben estar activas (409 si no).

---
### Encabezados Requeridos (Headers)
* Authorization: Bearer {token_jwt}
* Content-Type: application/json

---
### JSON de Prueba VÁLIDO (Copiar y Pegar en Swagger)
```json
{
  ""administrativeStatus"": ""active""
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones)
```json
{
  ""administrativeStatus"": ""unknown_status""
}
```

---
### Respuesta Exitosa (200 OK)
Retorna el DeviceResource con el estado administrativo actualizado.",
        OperationId = "ChangeDeviceAdministrativeStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado administrativo actualizado", typeof(DeviceResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Estado administrativo inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Dispositivo no encontrado")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Transición de estado inválida")]
    public async Task<IActionResult> ChangeDeviceAdministrativeStatus(
        Guid deviceId,
        [FromBody] ChangeDeviceAdministrativeStatusResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = ChangeDeviceAdministrativeStatusCommandFromResourceAssembler.ToCommandFromResource(
                organizationId, deviceId, resource);

            var device = await deviceCommandService.Handle(command);
            if (device == null) return NotFound();

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

    private IActionResult HandleInvalidOperation(InvalidOperationException exception)
    {
        return Conflict(new { message = exception.Message });
    }
}