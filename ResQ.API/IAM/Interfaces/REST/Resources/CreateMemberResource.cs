using System.ComponentModel.DataAnnotations;
using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Datos para que un Admin agregue un miembro (citizen o volunteer) a su organización.
/// </summary>
public record CreateMemberResource(
    string FirstName,
    string LastName,
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")] string Email,
    string Username,
    string Password,
    Roles Role);
