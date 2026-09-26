using ResQ.API.Profiles.Domain.Model.Aggregates;
using ResQ.API.Profiles.Interfaces.REST.Resources;

namespace ResQ.API.Profiles.Interfaces.REST.Transform;

public static class UserProfileResourceFromEntityAssembler
{
    public static UserProfileResource ToResourceFromEntity(UserProfile entity)
    {
        return new UserProfileResource(
            entity.Id,
            entity.Name.GetFullName(),
            entity.ContactInfo.Email,
            entity.ContactInfo.PhoneNumber
        );
    }
}
