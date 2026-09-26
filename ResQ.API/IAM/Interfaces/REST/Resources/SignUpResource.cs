using System.ComponentModel.DataAnnotations;
using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Objeto que representa los datos enviados para registrar un nuevo usuario en la plataforma.
/// </summary>
/// <param name="Username">El nombre de usuario deseado. Debe ser único en el sistema.</param>
/// <param name="FirstName">Nombre real del usuario.</param>
/// <param name="LastName">Apellido del usuario.</param>
/// <param name="Email">Correo electrónico de contacto. Debe ser un correo electrónico válido.</param>
/// <param name="Password">La contraseña en texto plano (será encriptada por el backend automáticamente).</param>
/// <param name="Role">El rol asignado al usuario (acepta: "citizen" o "volunteer").</param>
public record SignUpResource(
    string FirstName, 
    string LastName, 
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")] string Email, 
    string Username, 
    string Password, 
    Roles Role);
