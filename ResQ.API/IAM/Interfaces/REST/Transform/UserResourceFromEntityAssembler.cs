using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Interfaces.REST.Resources;

namespace ResQ.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembler for converting User entities to UserResource DTOs.
/// </summary>
public static class UserResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts a User entity to a UserResource.
    /// </summary>
    /// <param name="user">The User entity to convert.</param>
    /// <returns>A UserResource containing the user's data.</returns>
    public static UserResource ToResourceFromEntity(User user)
    {
        return new UserResource(user.Id, user.Username);
    }
}
