using ResQ.API.Profiles.Domain.Model.Aggregates;
using ResQ.API.Profiles.Domain.Model.Commands;

namespace ResQ.API.Profiles.Domain.Services;

public interface IUserProfileCommandService
{
    Task<UserProfile?> Handle(CreateUserProfileCommand command);
    Task<UserProfile?> Handle(UpdateContactInfoCommand command);
}
