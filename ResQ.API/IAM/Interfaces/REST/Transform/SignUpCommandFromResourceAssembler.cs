using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Interfaces.REST.Resources;

namespace ResQ.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembler for converting SignUpResource DTOs to SignUpCommand objects.
/// </summary>
public static class SignUpCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a SignUpResource to a SignUpCommand.
    /// </summary>
    /// <param name="resource">The SignUpResource containing sign-up data.</param>
    /// <returns>A SignUpCommand for user registration.</returns>
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        return new SignUpCommand(
            resource.FirstName,
            resource.LastName,
            resource.Email,
            resource.Username,
            resource.Password,
            resource.Role);
    }
}
