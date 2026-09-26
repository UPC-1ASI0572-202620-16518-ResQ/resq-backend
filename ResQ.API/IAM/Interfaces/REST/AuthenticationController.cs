using System.Net.Mime;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.IAM.Interfaces.REST.Resources;
using ResQ.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ResQ.API.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(IUserCommandService userCommandService) : ControllerBase
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
                      "- <b>400/500</b>: Credenciales inválidas o error interno.",
        OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "Autenticación exitosa. Retorna el usuario y el token JWT.", typeof(AuthenticatedUserResource))]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);
        var authenticatedUser = await userCommandService.Handle(signInCommand);
        var resource =
            AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(authenticatedUser.user,
                authenticatedUser.token);
        return Ok(resource);
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
                      "- <b>Role</b>: Define el tipo de acceso en el sistema. Los roles permitidos son:<br>" +
                      "  &nbsp;&nbsp;👉 <code>citizen</code> (Ciudadano que reporta emergencias)<br>" +
                      "  &nbsp;&nbsp;👉 <code>volunteer</code> (Voluntario que atiende reportes)<br><br>" +
                      "**Nota:** El rol debe enviarse exactamente como los valores permitidos (ignora mayúsculas/minúsculas).",
        OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status200OK, "El usuario fue creado y registrado exitosamente en la base de datos.")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource);
        await userCommandService.Handle(signUpCommand);
        return Ok(new { message = "User created successfully" });
    }
}
