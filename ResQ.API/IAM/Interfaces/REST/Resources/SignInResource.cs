namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Objeto que representa las credenciales enviadas para iniciar sesión.
/// </summary>
/// <param name="Username">El nombre de usuario previamente registrado.</param>
/// <param name="Password">La contraseña correspondiente al usuario.</param>
public record SignInResource(string Username, string Password);
