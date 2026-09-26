using ResQ.API.Profiles.Domain.Model.Commands;
using ResQ.API.Profiles.Domain.Services;

namespace ResQ.API.Profiles.Interfaces.ACL;

public class ProfilesContextFacade(IUserProfileCommandService userProfileCommandService) : IProfilesContextFacade
{
    public async Task<int> CreateUserProfile(int userId, string firstName, string lastName, string email)
    {
        var createProfileCommand = new CreateUserProfileCommand(userId, firstName, lastName, email);
        var userProfile = await userProfileCommandService.Handle(createProfileCommand);
        return userProfile?.Id ?? 0;
    }
}
