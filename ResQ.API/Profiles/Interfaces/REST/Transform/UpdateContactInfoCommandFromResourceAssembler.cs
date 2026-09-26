using ResQ.API.Profiles.Domain.Model.Commands;
using ResQ.API.Profiles.Interfaces.REST.Resources;

namespace ResQ.API.Profiles.Interfaces.REST.Transform;

public static class UpdateContactInfoCommandFromResourceAssembler
{
    public static UpdateContactInfoCommand ToCommandFromResource(int userId, UpdateContactInfoResource resource)
    {
        return new UpdateContactInfoCommand(userId, resource.Email, resource.PhoneNumber);
    }
}
