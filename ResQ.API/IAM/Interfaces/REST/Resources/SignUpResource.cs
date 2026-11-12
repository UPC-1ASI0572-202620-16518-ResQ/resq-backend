using System.ComponentModel.DataAnnotations;

namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Datos para registrar una nueva organización junto con su primer usuario (rol Admin).
/// </summary>
/// <param name="FirstName">Nombre real del usuario.</param>
/// <param name="LastName">Apellido del usuario.</param>
/// <param name="Email">Correo electrónico de contacto. Debe ser un correo electrónico válido.</param>
/// <param name="Username">El nombre de usuario deseado. Debe ser único en el sistema.</param>
/// <param name="Password">Contraseña en texto plano: 8 a 72 caracteres, con al menos una letra y un número.</param>
/// <param name="OrganizationName">Nombre de la organización a crear (opcional; por defecto "{username} organization").</param>
public record SignUpResource(
    string FirstName,
    string LastName,
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")] string Email,
    string Username,
    string Password,
    string? OrganizationName = null);
