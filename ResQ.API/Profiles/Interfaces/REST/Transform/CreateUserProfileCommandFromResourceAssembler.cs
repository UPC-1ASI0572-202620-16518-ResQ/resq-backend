using ResQ.API.Profiles.Domain.Model.Commands;
using ResQ.API.Profiles.Interfaces.REST.Resources;

namespace ResQ.API.Profiles.Interfaces.REST.Transform;

public static class CreateUserProfileCommandFromResourceAssembler
{
    public static CreateUserProfileCommand ToCommandFromResource(int userId, CreateUserProfileResource resource)
    {
        return new CreateUserProfileCommand(userId, resource.FirstName, resource.LastName, resource.Email);
    }
}
