using Microsoft.AspNetCore.Mvc;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;
using ResQ.API.Building_Management.Domain.Services;
using ResQ.API.Building_Management.Interfaces.REST.Resources;
using ResQ.API.Building_Management.Interfaces.REST.Transform;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Building_Management.Interfaces.REST;

/// <summary>
/// Controlador REST para la gestión de edificaciones y zonas (Building Management Bounded Context).
/// </summary>
[ApiController]
[Route("api/v1/buildings")]
[Produces("application/json")]
[Authorize]
public class BuildingsController(
    IBuildingCommandService buildingCommandService,
    IBuildingQueryService buildingQueryService) : ControllerBase
{
    // ══════════════════════════════════════════════════════════════════════════
    // BUILDING ENDPOINTS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registra una nueva edificación activa dentro de la organización autenticada.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Registrar edificación",
        Description = @"**Propósito:**
Registra una nueva edificación física en la organización. La edificación se crea automáticamente en estado **Active** con versión `1` y lista vacía de zonas.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}` (Obtenido en `/api/v1/authentication/sign-in`)
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO (Copiar y Pegar en Swagger)
```json
{
  ""buildingCode"": ""BLD-CENTRAL-01"",
  ""name"": ""Edificio Central de Operaciones"",
  ""description"": ""Sede principal corporativa con centro de monitoreo y oficinas"",
  ""address"": {
    ""streetAddress"": ""Av. Javier Prado Este 456"",
    ""district"": ""San Isidro"",
    ""city"": ""Lima"",
    ""countryCode"": ""PE""
  }
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones del sistema)
```json
{
  ""buildingCode"": ""bld central 01!"",
  ""name"": """",
  ""description"": ""Descripción válida"",
  ""address"": {
    ""streetAddress"": """",
    ""district"": ""San Isidro"",
    ""city"": ""Lima"",
    ""countryCode"": ""PER""
  }
}
```
**¿Por qué este JSON será RECHAZADO (400 Bad Request)?**
1. `buildingCode`: contiene caracteres en minúscula, espacios y símbolos no permitidos (solo mayúsculas, números y guiones; mín. 2 caracteres).
2. `name`: está vacío (campo obligatorio, máx. 120 caracteres).
3. `address.streetAddress`: está vacío (campo obligatorio, máx. 200 caracteres).
4. `address.countryCode`: tiene 3 caracteres (`PER`), pero la norma ISO 3166-1 alpha-2 exige exactamente 2 letras (ej. `PE`).

---
### Caso de Conflicto (409 Conflict)
Si intentas registrar un `buildingCode` que ya existe dentro de la misma organización, el endpoint retornará **409 Conflict** indicando que el código ya se encuentra registrado.

---
### Respuesta Exitosa (201 Created)
Retorna el objeto `BuildingResource` creado y el encabezado `Location: /api/v1/buildings/{buildingId}`.",
        OperationId = "RegisterBuilding")]
    [SwaggerResponse(StatusCodes.Status201Created, "Edificación registrada exitosamente", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Código de edificación duplicado en la organización")]
    public async Task<IActionResult> RegisterBuilding([FromBody] RegisterBuildingResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = RegisterBuildingCommandFromResourceAssembler
                .ToCommandFromResource(organizationId, resource);

            var building = await buildingCommandService.Handle(command);

            var buildingResource = BuildingResourceFromEntityAssembler.ToResourceFromEntity(building);

            return CreatedAtAction(nameof(GetBuildingById),
                new { buildingId = building.Id }, buildingResource);
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

    /// <summary>
    /// Lista edificaciones de la organización con paginación y filtro opcional por estado administrativo.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar edificaciones",
        Description = @"**Propósito:**
Obtiene una lista paginada de las edificaciones pertenecientes a la organización autenticada, con filtro opcional por estado administrativo (`active` o `inactive`).

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`

---
### Parámetros de Consulta (Query Params)
* `administrativeStatus` *(opcional)*: Filtra por estado (`active` o `inactive`). Si no se envía, lista todos.
* `page` *(opcional, default: 0)*: Índice de página base 0 (no puede ser negativo).
* `size` *(opcional, default: 20)*: Cantidad de registros por página (rango permitido: 1 a 100).

---
### Ejemplos de Consulta VÁLIDOS
* `GET /api/v1/buildings` (primeros 20 registros sin filtro)
* `GET /api/v1/buildings?administrativeStatus=active&page=0&size=10` (edificaciones activas, 10 por página)
* `GET /api/v1/buildings?administrativeStatus=inactive&page=0&size=5` (edificaciones inactivas)

---
### Ejemplos de Consulta INVÁLIDOS (400 Bad Request)
* `GET /api/v1/buildings?page=-1` (la página no puede ser negativa)
* `GET /api/v1/buildings?size=500` (el tamaño máximo permitido es 100)

---
### Respuesta Exitosa (200 OK)
Retorna un `BuildingPageResource` con `items`, `page`, `size`, `totalElements` y `totalPages`.",
        OperationId = "GetBuildings")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista paginada de edificaciones", typeof(BuildingPageResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parámetros de paginación o filtro inválidos")]
    public async Task<IActionResult> GetBuildings(
        [FromQuery] LocationAdministrativeStatus? administrativeStatus,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (page < 0) return BadRequest(new { message = "Page cannot be negative." });
        if (size is < 1 or > 100) return BadRequest(new { message = "Size must be between 1 and 100." });

        var query = BuildingQueriesFromRequestAssembler
            .ToGetBuildingsQuery(organizationId, administrativeStatus, page, size);

        var result = await buildingQueryService.Handle(query);
        var resource = BuildingPageResourceFromPageAssembler.ToResourceFromPage(result);

        return Ok(resource);
    }

    /// <summary>
    /// Obtiene los detalles de una edificación por su ID, incluyendo sus zonas visibles.
    /// </summary>
    [HttpGet("{buildingId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener edificación por ID",
        Description = @"**Propósito:**
Retorna la información completa de una edificación, incluyendo su dirección, estado administrativo, versión y la lista de zonas asociadas.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`

---
### Parámetro de Ruta (Path Param)
* `buildingId`: GUID de la edificación (ej. `9c7299c6-5154-4fea-afc5-8e58e0771819`).

---
### Respuestas
* **200 OK**: Retorna el `BuildingResource` con sus zonas.
* **404 Not Found**: La edificación no existe en la organización del usuario.",
        OperationId = "GetBuildingById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Edificación encontrada exitosamente", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación no encontrada")]
    public async Task<IActionResult> GetBuildingById(Guid buildingId)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var query = BuildingQueriesFromRequestAssembler
            .ToGetBuildingByIdQuery(organizationId, buildingId);

        var building = await buildingQueryService.Handle(query);
        if (building == null) return NotFound();

        var resource = BuildingResourceFromEntityAssembler.ToResourceFromEntity(building);
        return Ok(resource);
    }

    /// <summary>
    /// Actualiza los detalles descriptivos y dirección física de una edificación.
    /// </summary>
    [HttpPut("{buildingId:guid}/details")]
    [SwaggerOperation(
        Summary = "Actualizar detalles de edificación",
        Description = @"**Propósito:**
Actualiza el nombre, descripción o dirección de una edificación existente directamente.

**Actualización Parcial Soportada:**
Puedes enviar únicamente los campos que deseas modificar, o dejar los demás en blanco, null o con el valor por defecto de Swagger 'string'. El sistema ignorará cualquier campo vacío o 'string' y conservará los valores actuales de la edificación en la base de datos.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO - Cambiar solo el nombre (Copiar y Pegar en Swagger)
```json
{
  ""name"": ""Edificio Central Corporativo - Torre Norte""
}
```

---
### JSON de Prueba VÁLIDO - Con plantilla por defecto de Swagger (Solo modifica el nombre)
```json
{
  ""name"": ""Edificio Central Corporativo - Torre Norte"",
  ""description"": ""string"",
  ""address"": {
    ""streetAddress"": ""string"",
    ""district"": ""string"",
    ""city"": ""string"",
    ""countryCode"": ""string""
  }
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones del sistema)
```json
{
  ""address"": {
    ""countryCode"": ""PERU""
  }
}
```
**¿Por qué este JSON será RECHAZADO (400 Bad Request)?**
* `address.countryCode`: tiene 4 caracteres (`PERU`), solo se permiten exactamente 2 letras mayúsculas según norma ISO 3166-1 alpha-2.

---
### Respuesta Exitosa (200 OK)
Retorna el `BuildingResource` actualizado.",
        OperationId = "UpdateBuildingDetails")]
    [SwaggerResponse(StatusCodes.Status200OK, "Edificación actualizada exitosamente", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación no encontrada")]
    public async Task<IActionResult> UpdateBuildingDetails(
        Guid buildingId, [FromBody] UpdateBuildingDetailsResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = UpdateBuildingDetailsCommandFromResourceAssembler
                .ToCommandFromResource(organizationId, buildingId, resource);

            var building = await buildingCommandService.Handle(command);

            return Ok(BuildingResourceFromEntityAssembler.ToResourceFromEntity(building));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (InvalidOperationException e) { return HandleInvalidOperation(e); }
    }

    /// <summary>
    /// Cambia el estado administrativo (Active / Inactive) de una edificación.
    /// </summary>
    [HttpPut("{buildingId:guid}/administrative-status")]
    [SwaggerOperation(
        Summary = "Cambiar estado administrativo de edificación",
        Description = @"**Propósito:**
Activa o desactiva administrativamente una edificación.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO - Para Desactivar (Copiar y Pegar en Swagger)
```json
{
  ""administrativeStatus"": ""inactive""
}
```

---
### JSON de Prueba VÁLIDO - Para Reactivar (Copiar y Pegar en Swagger)
```json
{
  ""administrativeStatus"": ""active""
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones)
```json
{
  ""administrativeStatus"": ""archived""
}
```
*(Solo se aceptan los valores `active` o `inactive`)*.

---
### Impacto en el Dominio al Desactivar una Edificación
1. **Zonas bloqueadas:** Ninguna de sus zonas estará disponible para asignación de dispositivos (`availableForAssignment` pasa a `false`).
2. **Creación bloqueada:** No se podrán agregar nuevas zonas a un edificio inactivo (retornará `409 Conflict`).
3. **Activación de zonas bloqueada:** No se puede activar una zona si su edificación está inactiva.

---
### Respuesta Exitosa (200 OK)
Retorna la edificación con su nuevo estado administrativo.",
        OperationId = "ChangeBuildingAdministrativeStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado administrativo actualizado", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Estado administrativo no reconocido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación no encontrada")]
    public async Task<IActionResult> ChangeBuildingAdministrativeStatus(
        Guid buildingId, [FromBody] ChangeBuildingAdministrativeStatusResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = new Domain.Model.Commands.ChangeBuildingAdministrativeStatusCommand(
                organizationId, buildingId, resource.AdministrativeStatus);

            var building = await buildingCommandService.Handle(command);

            return Ok(BuildingResourceFromEntityAssembler.ToResourceFromEntity(building));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException e) { return HandleInvalidOperation(e); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ZONE ENDPOINTS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Agrega una nueva zona activa a una edificación activa.
    /// </summary>
    [HttpPost("{buildingId:guid}/zones")]
    [SwaggerOperation(
        Summary = "Agregar zona a edificación",
        Description = @"**Propósito:**
Agrega un área o zona física (ej. Sala de Servidores, Cocina, Laboratorio) a una edificación existente y activa. La zona se crea automáticamente en estado **Active**.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO (Copiar y Pegar en Swagger)
```json
{
  ""zoneCode"": ""ZON-SERVER-01"",
  ""name"": ""Sala Principal de Servidores"",
  ""description"": ""Data center principal con racks de telecomunicaciones y SAI"",
  ""floorLabel"": ""Sótano 1""
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones del sistema)
```json
{
  ""zoneCode"": ""zona servidores!"",
  ""name"": """",
  ""description"": ""Descripción válida"",
  ""floorLabel"": ""Este piso tiene un nombre extremadamente largo que supera los cincuenta caracteres permitidos por la validación""
}
```
**¿Por qué este JSON será RECHAZADO (400 Bad Request)?**
1. `zoneCode`: contiene minúsculas, espacios y caracteres especiales (solo se permiten mayúsculas, números y guiones).
2. `name`: está vacío (obligatorio, máx. 120 caracteres).
3. `floorLabel`: supera los 50 caracteres permitidos.

---
### Casos de Conflicto de Negocio (409 Conflict)
1. **Edificación Inactiva:** Si la edificación está en estado `inactive`, retornará **409 Conflict**: `""Cannot add a zone to an inactive building.""`.
2. **Código de Zona Duplicado:** Si ya existe una zona con el mismo `zoneCode` dentro de la edificación, retornará **409 Conflict**: `""A zone with code 'ZON-SERVER-01' already exists in this building.""`.

---
### Respuesta Exitosa (201 Created)
Retorna la edificación conteniendo la nueva zona en la colección `zones`.",
        OperationId = "AddZoneToBuilding")]
    [SwaggerResponse(StatusCodes.Status201Created, "Zona agregada exitosamente", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Código de zona duplicado o edificación inactiva")]
    public async Task<IActionResult> AddZoneToBuilding(
        Guid buildingId, [FromBody] AddZoneToBuildingResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = new Domain.Model.Commands.AddZoneToBuildingCommand(
                organizationId, buildingId, resource.ZoneCode,
                resource.Name, resource.Description, resource.FloorLabel);

            var building = await buildingCommandService.Handle(command);

            var addedZone = building.Zones.Last();

            var buildingResource = BuildingResourceFromEntityAssembler.ToResourceFromEntity(building);

            return CreatedAtAction(nameof(GetZoneById),
                new { buildingId = building.Id, zoneId = addedZone.Id },
                buildingResource);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
    }

    /// <summary>
    /// Lista las zonas pertenecientes a una edificación con filtro opcional por estado.
    /// </summary>
    [HttpGet("{buildingId:guid}/zones")]
    [SwaggerOperation(
        Summary = "Listar zonas de edificación",
        Description = @"**Propósito:**
Obtiene la lista paginada de zonas pertenecientes a una edificación, calculando el campo `availableForAssignment`.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`

---
### Parámetros
* `buildingId` *(ruta)*: GUID de la edificación.
* `administrativeStatus` *(query, opcional)*: `active` o `inactive`.
* `page` *(query, opcional, default: 0)*: Índice de página.
* `size` *(query, opcional, default: 20)*: Registros por página (1 a 100).

---
### Campo de Negocio: availableForAssignment
Cada zona incluye la propiedad booleana `availableForAssignment`:
* Retorna `true` únicamente si la **edificación está activa** Y la **zona está activa**.
* Si la edificación o la zona están inactivas, retorna `false` (lo que impide asignar dispositivos a esta zona).

---
### Respuesta Exitosa (200 OK)
Retorna `ZonePageResource` con la lista de zonas paginadas.",
        OperationId = "GetZonesByBuildingId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista paginada de zonas", typeof(ZonePageResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parámetros de paginación inválidos")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación no encontrada")]
    public async Task<IActionResult> GetZonesByBuildingId(
        Guid buildingId,
        [FromQuery] LocationAdministrativeStatus? administrativeStatus,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        if (page < 0) return BadRequest(new { message = "Page cannot be negative." });
        if (size is < 1 or > 100) return BadRequest(new { message = "Size must be between 1 and 100." });

        var building = await buildingQueryService.Handle(
            BuildingQueriesFromRequestAssembler.ToGetBuildingByIdQuery(organizationId, buildingId));

        if (building == null) return NotFound();

        var query = BuildingQueriesFromRequestAssembler
            .ToGetZonesByBuildingIdQuery(organizationId, buildingId,
                administrativeStatus, page, size);

        var result = await buildingQueryService.Handle(query);
        var resource = ZonePageResourceFromPageAssembler.ToResourceFromPage(result, building);

        return Ok(resource);
    }

    /// <summary>
    /// Obtiene los detalles de una zona específica dentro de una edificación.
    /// </summary>
    [HttpGet("{buildingId:guid}/zones/{zoneId:guid}")]
    [SwaggerOperation(
        Summary = "Obtener zona por ID",
        Description = @"**Propósito:**
Obtiene los detalles de una zona física específica dentro de una edificación.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`

---
### Parámetros de Ruta (Path Params)
* `buildingId`: GUID de la edificación padre.
* `zoneId`: GUID de la zona buscada.

---
### Respuestas
* **200 OK**: Retorna el `ZoneResource` con código, nombre, piso, estado y `availableForAssignment`.
* **404 Not Found**: Si la edificación o la zona no existen.",
        OperationId = "GetZoneById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Zona encontrada exitosamente", typeof(ZoneResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación o zona no encontrada")]
    public async Task<IActionResult> GetZoneById(Guid buildingId, Guid zoneId)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        var building = await buildingQueryService.Handle(
            BuildingQueriesFromRequestAssembler.ToGetBuildingByIdQuery(organizationId, buildingId));

        if (building == null) return NotFound();

        var zone = await buildingQueryService.Handle(
            BuildingQueriesFromRequestAssembler.ToGetZoneByIdQuery(organizationId, buildingId, zoneId));

        if (zone == null) return NotFound();

        var resource = ZoneResourceFromEntityAssembler.ToResourceFromEntity(zone, building);
        return Ok(resource);
    }

    /// <summary>
    /// Actualiza los datos descriptivos y piso de una zona.
    /// </summary>
    [HttpPut("{buildingId:guid}/zones/{zoneId:guid}/details")]
    [SwaggerOperation(
        Summary = "Actualizar datos de zona",
        Description = @"**Propósito:**
Actualiza el nombre, descripción y/o etiqueta de piso de una zona perteneciente a una edificación directamente.

**Actualización Parcial Soportada:**
Puedes enviar únicamente los campos que deseas modificar o dejar los demás como null, vacíos o 'string'. Los campos no enviados conservarán su valor actual.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO - Cambiar solo el nombre (Copiar y Pegar en Swagger)
```json
{
  ""name"": ""Sala de Servidores - Tier 3""
}
```

---
### JSON de Prueba VÁLIDO - Con plantilla Swagger
```json
{
  ""name"": ""Sala de Servidores - Tier 3"",
  ""description"": ""string"",
  ""floorLabel"": ""string""
}
```

---
### Respuesta Exitosa (200 OK)
Retorna la edificación con la zona actualizada.",
        OperationId = "UpdateZone")]
    [SwaggerResponse(StatusCodes.Status200OK, "Zona actualizada exitosamente", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos en el payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación o zona no encontrada")]
    public async Task<IActionResult> UpdateZone(
        Guid buildingId, Guid zoneId, [FromBody] UpdateZoneResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = new Domain.Model.Commands.UpdateZoneCommand(
                organizationId, buildingId, zoneId,
                resource.Name, resource.Description, resource.FloorLabel);

            var building = await buildingCommandService.Handle(command);

            return Ok(BuildingResourceFromEntityAssembler.ToResourceFromEntity(building));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (InvalidOperationException e) { return HandleInvalidOperation(e); }
    }

    /// <summary>
    /// Cambia el estado administrativo (Active / Inactive) de una zona específica.
    /// </summary>
    [HttpPut("{buildingId:guid}/zones/{zoneId:guid}/administrative-status")]
    [SwaggerOperation(
        Summary = "Cambiar estado administrativo de zona",
        Description = @"**Propósito:**
Activa o desactiva una zona específica. La activación de una zona requiere de forma obligatoria que su edificación padre se encuentre en estado **Active**.

---
### Encabezados Requeridos (Headers)
* `Authorization`: `Bearer {token_jwt}`
* `Content-Type`: `application/json`

---
### JSON de Prueba VÁLIDO - Para Desactivar Zona (Copiar y Pegar en Swagger)
```json
{
  ""administrativeStatus"": ""inactive""
}
```

---
### JSON de Prueba VÁLIDO - Para Reactivar Zona (Copiar y Pegar en Swagger)
```json
{
  ""administrativeStatus"": ""active""
}
```

---
### JSON de Prueba INVÁLIDO (Para probar validaciones)
```json
{
  ""administrativeStatus"": ""deleted""
}
```
*(Solo se aceptan los valores `active` o `inactive`)*.

---
### Regla de Conflicto de Negocio (409 Conflict)
Si la **edificación se encuentra inactiva** y se intenta activar una zona, el endpoint rechazará la operación retornando **409 Conflict**:
`""Cannot activate a zone in an inactive building.""`.

---
### Respuesta Exitosa (200 OK)
Retorna la edificación con el estado de la zona actualizado.",
        OperationId = "ChangeZoneAdministrativeStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estado de zona actualizado exitosamente", typeof(BuildingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Estado administrativo no reconocido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Edificación o zona no encontrada")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Conflicto: la edificación padre está inactiva")]
    public async Task<IActionResult> ChangeZoneAdministrativeStatus(
        Guid buildingId, Guid zoneId,
        [FromBody] ChangeZoneAdministrativeStatusResource resource)
    {
        if (!TryGetOrganizationId(out var organizationId)) return Unauthorized();

        try
        {
            var command = new Domain.Model.Commands.ChangeZoneAdministrativeStatusCommand(
                organizationId, buildingId, zoneId,
                resource.AdministrativeStatus);

            var building = await buildingCommandService.Handle(command);

            return Ok(BuildingResourceFromEntityAssembler.ToResourceFromEntity(building));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException e) { return HandleInvalidOperation(e); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS PRIVADOS
    // ══════════════════════════════════════════════════════════════════════════

    private bool TryGetOrganizationId(out Guid organizationId)
    {
        organizationId = Guid.Empty;

        if (HttpContext.Items["OrganizationId"] is Guid id && id != Guid.Empty)
        {
            organizationId = id;
            return true;
        }

        if (HttpContext.Items["OrganizationId"] is string value &&
            Guid.TryParse(value, out id) && id != Guid.Empty)
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
