using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Interfaces.REST.Resources;

namespace ResQ.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembler for converting SignInResource DTOs to SignInCommand objects.
/// </summary>
public static class SignInCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a SignInResource to a SignInCommand.
    /// </summary>
    /// <param name="resource">The SignInResource containing sign-in data.</param>
    /// <returns>A SignInCommand for user authentication.</returns>
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        return new SignInCommand(resource.Username, resource.Password);
    }
}
