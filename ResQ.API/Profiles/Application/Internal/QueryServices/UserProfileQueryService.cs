using ResQ.API.Profiles.Domain.Model.Aggregates;
using ResQ.API.Profiles.Domain.Model.Queries;
using ResQ.API.Profiles.Domain.Repositories;
using ResQ.API.Profiles.Domain.Services;

namespace ResQ.API.Profiles.Application.Internal.QueryServices;

public class UserProfileQueryService(IUserProfileRepository userProfileRepository) : IUserProfileQueryService
{
    public async Task<UserProfile?> Handle(GetUserProfileByIdQuery query)
    {
        return await userProfileRepository.FindByIdAsync(query.UserId);
    }
}
