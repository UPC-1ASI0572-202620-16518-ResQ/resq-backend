using System.Net.Mime;
using ResQ.API.IAM.Domain.Model.Queries;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.IAM.Interfaces.REST.Resources;
using ResQ.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.IAM.Interfaces.REST;

/// <summary>
///     Users REST controller for user management.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Users endpoints")]
public class UsersController(IUserQueryService userQueryService) : ControllerBase
{
    /// <summary>
    ///     Obtener un usuario por su ID.
    /// </summary>
    /// <param name="id">El identificador único del usuario.</param>
    /// <returns>Retorna un <see cref="IActionResult" /> que contiene el <see cref="UserResource" /> si se encuentra.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Obtener usuario por ID",
        Description = "Busca un usuario específico mediante su ID en la base de datos.<br><br>" +
                      "**Seguridad:**<br>" +
                      "- Requiere autenticación (Token JWT válido en el header <code>Authorization: Bearer {token}</code>).<br><br>" +
                      "**Respuestas:**<br>" +
                      "- <b>200 OK</b>: Devuelve el objeto del usuario sin información sensible (como la contraseña).<br>" +
                      "- <b>404 Not Found</b>: Si el ID proporcionado no existe en el sistema.",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "El usuario fue encontrado exitosamente.", typeof(UserResource))]
    public async Task<IActionResult> GetUserById(int id)
    {
        var getUserByIdQuery = new GetUserByIdQuery(id);
        var user = await userQueryService.Handle(getUserByIdQuery);
        if (user is null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(userResource);
    }

    /// <summary>
    ///     Obtener el listado de todos los usuarios.
    /// </summary>
    /// <returns>Retorna una colección de <see cref="UserResource" /> de todos los usuarios registrados.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtener todos los usuarios",
        Description = "Devuelve una lista completa de todos los usuarios registrados en el sistema.<br><br>" +
                      "**Seguridad:**<br>" +
                      "- Requiere autenticación (Token JWT válido en el header).<br><br>" +
                      "**Respuestas:**<br>" +
                      "- <b>200 OK</b>: Devuelve el listado completo (id, username) filtrando información sensible.",
        OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "Los usuarios fueron encontrados.", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers()
    {
        var getAllUsersQuery = new GetAllUsersQuery();
        var users = await userQueryService.Handle(getAllUsersQuery);
        var userResources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(userResources);
    }

    /// <summary>
    ///     Obtener usuarios filtrados por rol.
    /// </summary>
    /// <param name="role">El rol por el cual filtrar a los usuarios (ej: citizen, volunteer).</param>
    /// <returns>Retorna una colección de <see cref="UserResource" /> que pertenecen al rol especificado.</returns>
    [HttpGet("role/{role}")]
    [SwaggerOperation(
        Summary = "Filtrar usuarios por rol",
        Description = "Obtiene una lista de usuarios que coinciden con un rol específico.<br><br>" +
                      "**Seguridad:**<br>" +
                      "- Requiere autenticación (Token JWT válido en el header).<br><br>" +
                      "**Roles Permitidos (case-insensitive):**<br>" +
                      "- <code>citizen</code>: Para listar usuarios ciudadanos.<br>" +
                      "- <code>volunteer</code>: Para listar usuarios voluntarios.<br><br>" +
                      "**Respuestas:**<br>" +
                      "- <b>200 OK</b>: Devuelve la lista filtrada de usuarios.<br>" +
                      "- <b>400 Bad Request</b>: Si el rol ingresado en la URL no es válido dentro del dominio del sistema.",
        OperationId = "GetUsersByRole")]
    [SwaggerResponse(StatusCodes.Status200OK, "Los usuarios con el rol especificado fueron encontrados.", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        if (!Enum.TryParse<Roles>(role, true, out var userRole))
        {
            return BadRequest($"Invalid role: {role}");
        }
        var getUserByRoleQuery = new GetUserByRoleQuery(userRole);
        var users = await userQueryService.Handle(getUserByRoleQuery);
        var userResources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(userResources);
    }
}
