using ResQ.API.Profiles.Domain.Model.Aggregates;
using ResQ.API.Profiles.Domain.Model.Queries;

namespace ResQ.API.Profiles.Domain.Services;

public interface IUserProfileQueryService
{
    Task<UserProfile?> Handle(GetUserProfileByIdQuery query);
}
