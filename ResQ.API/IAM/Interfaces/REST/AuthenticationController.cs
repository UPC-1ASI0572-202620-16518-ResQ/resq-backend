using System.Net.Mime;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.IAM.Interfaces.REST.Resources;
using ResQ.API.IAM.Interfaces.REST.Transform;
using ResQ.API.Profiles.Interfaces.ACL;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(
    IUserCommandService userCommandService,
    IProfilesContextFacade profilesContextFacade) : ControllerBase
{
    /// <summary>
    ///     Iniciar sesión (Autenticación de usuario).
    /// </summary>
    /// <param name="signInResource">El payload JSON que contiene el username y password.</param>
    /// <returns>
    ///     Retorna un objeto <see cref="AuthenticatedUserResource" /> que incluye el token JWT y los datos del usuario.
    /// </returns>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Iniciar Sesión",
        Description = "Permite a un usuario existente autenticarse en la plataforma.<br><br>" +
                      "**Proceso de validación:**<br>" +
                      "- Verifica que el <b>username</b> exista en la base de datos.<br>" +
                      "- Compara el <b>password</b> enviado en texto plano contra el hash seguro (BCrypt) almacenado.<br>" +
                      "- Si es exitoso, genera y retorna un <b>Token JWT</b> firmado y válido por 7 días, el cual debe ser usado " +
                      "en los demás endpoints protegidos en el header <code>Authorization: Bearer {token}</code>.<br><br>" +
                      "**Respuestas:**<br>" +
                      "- <b>200 OK</b>: Autenticación exitosa.<br>" +
                      "- <b>401 Unauthorized</b>: Usuario o contraseña incorrectos.",
        OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "Autenticación exitosa. Retorna el usuario y el token JWT.", typeof(AuthenticatedUserResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Usuario o contraseña incorrectos.")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        try
        {
            var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);
            var authenticatedUser = await userCommandService.Handle(signInCommand);
            var resource =
                AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(authenticatedUser.user,
                    authenticatedUser.token);
            return Ok(resource);
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(new { message = e.Message });
        }
    }

    /// <summary>
    ///     Registrar un nuevo usuario en la plataforma.
    /// </summary>
    /// <param name="signUpResource">El payload JSON con los datos requeridos (username, password y role).</param>
    /// <returns>Mensaje de confirmación de creación exitosa.</returns>
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Registrar nuevo usuario",
        Description = "Crea un nuevo registro de usuario en el sistema.<br><br>" +
                      "**Reglas y Validaciones:**<br>" +
                      "- <b>Username</b>: Debe ser único. Si el usuario ya existe, se rechazará la petición.<br>" +
                      "- <b>Password</b>: El sistema aplicará un algoritmo de hashing fuerte (BCrypt) antes de guardarlo en la base de datos. ¡Nunca se guarda en texto plano!<br>" +
                      "- <b>FirstName y LastName</b>: Deben ser proporcionados para el perfil base.<br>" +
                      "- <b>Email</b>: Debe tener un formato de correo válido (ej. texto antes y después del '@', y un dominio con punto).<br>" +
                      "- <b>Role</b>: Define el tipo de acceso en el sistema. Los roles permitidos son:<br>" +
                      "  &nbsp;&nbsp;👉 <code>citizen</code> (Ciudadano que reporta emergencias)<br>" +
                      "  &nbsp;&nbsp;👉 <code>volunteer</code> (Voluntario que atiende reportes)<br><br>" +
                      "**Nota:** El rol debe enviarse exactamente como los valores permitidos (ignora mayúsculas/minúsculas).<br><br>" +
                      "**Creación Automática:** Al registrarse exitosamente en IAM, se creará automáticamente un **Perfil de Usuario** " +
                      "asociado con el Nombre, Apellido y Correo proporcionados.",
        OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status200OK, "El usuario fue creado y registrado exitosamente en la base de datos.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos de registro inválidos (campos vacíos, email o rol inválido).")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "El username ya está registrado.")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        try
        {
            var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource);
            var userId = await userCommandService.Handle(signUpCommand);

            // Auto-create profile in the Profiles bounded context
            await profilesContextFacade.CreateUserProfile(userId, signUpResource.FirstName.Trim(),
                signUpResource.LastName.Trim(), signUpResource.Email.Trim());

            return Ok(new { message = "User created successfully" });
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
}
