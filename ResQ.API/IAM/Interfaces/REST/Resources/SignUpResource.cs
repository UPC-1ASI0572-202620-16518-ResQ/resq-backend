using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Objeto que representa los datos enviados para registrar un nuevo usuario en la plataforma.
/// </summary>
/// <param name="Username">El nombre de usuario deseado. Debe ser único en el sistema.</param>
/// <param name="Password">La contraseña en texto plano (será encriptada por el backend automáticamente).</param>
/// <param name="Role">El rol asignado al usuario (acepta: "citizen" o "volunteer").</param>
public record SignUpResource(string Username, string Password, Roles Role);
