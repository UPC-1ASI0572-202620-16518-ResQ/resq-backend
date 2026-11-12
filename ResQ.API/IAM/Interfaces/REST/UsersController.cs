using System.Net.Mime;
using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Domain.Model.Queries;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.IAM.Interfaces.REST.Resources;
using ResQ.API.IAM.Interfaces.REST.Transform;
using ResQ.API.Profiles.Interfaces.ACL;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.IAM.Interfaces.REST;

/// <summary>
///     Users of the authenticated user's organization. Users of other organizations are never visible.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Users endpoints")]
public class UsersController(
    IUserQueryService userQueryService,
    IUserCommandService userCommandService,
    IProfilesContextFacade profilesContextFacade) : ControllerBase
{
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Obtener usuario por ID",
        Description = "Busca un usuario de la **misma organización** del usuario autenticado.<br><br>" +
                      "**Respuestas:**<br>" +
                      "- <b>200 OK</b>: Devuelve id, username, rol y organización (sin la contraseña).<br>" +
                      "- <b>404 Not Found</b>: Si el ID no existe o pertenece a otra organización.",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "El usuario fue encontrado exitosamente.", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Usuario no encontrado en la organización.")]
    public async Task<IActionResult> GetUserById(int id)
    {
        if (HttpContext.Items["OrganizationId"] is not Guid organizationId) return Unauthorized();

        var user = await userQueryService.Handle(new GetUserByIdQuery(id));
        if (user is null || user.OrganizationId != organizationId) return NotFound();

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    [HttpGet]
    [Authorize(Roles.Admin)]
    [SwaggerOperation(
        Summary = "Obtener los usuarios de mi organización",
        Description = "Devuelve los usuarios de la organización del usuario autenticado.<br><br>" +
                      "**Seguridad:** solo rol <code>admin</code>.",
        OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "Los usuarios fueron encontrados.", typeof(IEnumerable<UserResource>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El usuario no es admin.")]
    public async Task<IActionResult> GetAllUsers()
    {
        if (HttpContext.Items["OrganizationId"] is not Guid organizationId) return Unauthorized();

        var users = await userQueryService.Handle(new GetAllUsersQuery(organizationId));
        return Ok(users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("role/{role}")]
    [Authorize(Roles.Admin)]
    [SwaggerOperation(
        Summary = "Filtrar usuarios de mi organización por rol",
        Description = "Roles permitidos (case-insensitive): <code>citizen</code>, <code>volunteer</code>, <code>admin</code>.<br><br>" +
                      "**Seguridad:** solo rol <code>admin</code>.",
        OperationId = "GetUsersByRole")]
    [SwaggerResponse(StatusCodes.Status200OK, "Los usuarios con el rol especificado fueron encontrados.", typeof(IEnumerable<UserResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Rol inválido.")]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        if (HttpContext.Items["OrganizationId"] is not Guid organizationId) return Unauthorized();

        if (!Enum.TryParse<Roles>(role, true, out var userRole) || !Enum.IsDefined(userRole) ||
            int.TryParse(role, out _))
            return BadRequest(new { message = $"Invalid role: {role}" });

        var users = await userQueryService.Handle(new GetUserByRoleQuery(organizationId, userRole));
        return Ok(users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(Roles.Admin)]
    [SwaggerOperation(
        Summary = "Agregar un miembro a mi organización",
        Description = "Crea un usuario <code>citizen</code> o <code>volunteer</code> dentro de la organización del admin " +
                      "autenticado, y su perfil.<br><br>" +
                      "**Contraseña:** 8 a 72 caracteres, con al menos una letra y un número.<br>" +
                      "**Seguridad:** solo rol <code>admin</code>.",
        OperationId = "CreateMember")]
    [SwaggerResponse(StatusCodes.Status201Created, "El miembro fue creado.", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos o rol no permitido.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El usuario no es admin.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "El username ya está registrado.")]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberResource resource)
    {
        if (HttpContext.Items["OrganizationId"] is not Guid organizationId) return Unauthorized();

        try
        {
            var command = new CreateMemberCommand(organizationId, resource.FirstName, resource.LastName,
                resource.Email, resource.Username, resource.Password, resource.Role);
            var userId = await userCommandService.Handle(command);

            await profilesContextFacade.CreateUserProfile(userId, resource.FirstName.Trim(),
                resource.LastName.Trim(), resource.Email.Trim());

            var user = await userQueryService.Handle(new GetUserByIdQuery(userId));
            return CreatedAtAction(nameof(GetUserById), new { id = userId },
                UserResourceFromEntityAssembler.ToResourceFromEntity(user!));
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return Conflict(new { message = e.Message });
        }
    }
}
