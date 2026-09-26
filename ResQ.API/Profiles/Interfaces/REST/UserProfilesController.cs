using Microsoft.AspNetCore.Mvc;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Profiles.Domain.Model.Queries;
using ResQ.API.Profiles.Domain.Services;
using ResQ.API.Profiles.Interfaces.REST.Resources;
using ResQ.API.Profiles.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.Profiles.Interfaces.REST;

[ApiController]
[Route("api/v1/profiles")]
[Produces("application/json")]
[Authorize] // Requires JWT from IAM
public class UserProfilesController(
    IUserProfileCommandService userProfileCommandService,
    IUserProfileQueryService userProfileQueryService)
    : ControllerBase
{
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Obtener mi perfil",
        Description = "Obtiene los detalles del perfil del usuario actualmente autenticado (basado en el Token JWT).",
        OperationId = "GetMyProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "El perfil fue encontrado", typeof(UserProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Perfil no encontrado")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = HttpContext.Items["User"] as ResQ.API.IAM.Domain.Model.Aggregates.User;
        if (user == null) return Unauthorized();

        var query = new GetUserProfileByIdQuery(user.Id);
        var userProfile = await userProfileQueryService.Handle(query);
        if (userProfile == null) return NotFound();

        var resource = UserProfileResourceFromEntityAssembler.ToResourceFromEntity(userProfile);
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Obtener un perfil por ID",
        Description = "Obtiene los detalles del perfil de un usuario.",
        OperationId = "GetUserProfileById")]
    [SwaggerResponse(StatusCodes.Status200OK, "El perfil fue encontrado", typeof(UserProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Perfil no encontrado")]
    public async Task<IActionResult> GetUserProfileById(int id)
    {
        var query = new GetUserProfileByIdQuery(id);
        var userProfile = await userProfileQueryService.Handle(query);
        if (userProfile == null) return NotFound();

        var resource = UserProfileResourceFromEntityAssembler.ToResourceFromEntity(userProfile);
        return Ok(resource);
    }

    [HttpPut("contact-info")]
    [SwaggerOperation(
        Summary = "Actualizar información de contacto",
        Description = "Actualiza el correo y teléfono del usuario autenticado.",
        OperationId = "UpdateContactInfo")]
    [SwaggerResponse(StatusCodes.Status200OK, "Información de contacto actualizada", typeof(UserProfileResource))]
    public async Task<IActionResult> UpdateContactInfo([FromBody] UpdateContactInfoResource resource)
    {
        var user = HttpContext.Items["User"] as ResQ.API.IAM.Domain.Model.Aggregates.User;
        if (user == null) return Unauthorized();

        var command = UpdateContactInfoCommandFromResourceAssembler.ToCommandFromResource(user.Id, resource);
        try
        {
            var userProfile = await userProfileCommandService.Handle(command);
            if (userProfile == null) return BadRequest();

            var userProfileResource = UserProfileResourceFromEntityAssembler.ToResourceFromEntity(userProfile);
            return Ok(userProfileResource);
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}
