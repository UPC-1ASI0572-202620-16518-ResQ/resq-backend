using ResQ.API.Shared.Domain.Repositories;
using ResQ.API.Profiles.Domain.Model.Aggregates;
using ResQ.API.Profiles.Domain.Model.Commands;
using ResQ.API.Profiles.Domain.Repositories;
using ResQ.API.Profiles.Domain.Services;

namespace ResQ.API.Profiles.Application.Internal.CommandServices;

public class UserProfileCommandService(
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork)
    : IUserProfileCommandService
{
    public async Task<UserProfile?> Handle(CreateUserProfileCommand command)
    {
        var existingProfile = await userProfileRepository.FindByIdAsync(command.UserId);
        if (existingProfile != null)
            throw new Exception("Profile already exists for this user.");

        var userProfile = new UserProfile(command.UserId, command.FirstName, command.LastName, command.Email);
        
        await userProfileRepository.AddAsync(userProfile);
        await unitOfWork.CompleteAsync();

        return userProfile;
    }

    public async Task<UserProfile?> Handle(UpdateContactInfoCommand command)
    {
        var userProfile = await userProfileRepository.FindByIdAsync(command.UserId);
        if (userProfile == null)
            throw new Exception("Profile not found");

        userProfile.UpdateContactInfo(command.NewEmail, command.NewPhoneNumber);
        
        userProfileRepository.Update(userProfile);
        await unitOfWork.CompleteAsync();
        
        return userProfile;
    }
}
