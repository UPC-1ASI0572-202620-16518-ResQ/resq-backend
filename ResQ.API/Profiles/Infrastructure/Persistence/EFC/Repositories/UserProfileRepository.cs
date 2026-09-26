using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.Profiles.Domain.Model.Aggregates;
using ResQ.API.Profiles.Domain.Repositories;

namespace ResQ.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

public class UserProfileRepository(AppDbContext context) : BaseRepository<UserProfile>(context), IUserProfileRepository
{
}
