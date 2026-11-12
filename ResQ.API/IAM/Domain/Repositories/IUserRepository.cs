using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.IAM.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByUsernameAsync(string username);

    bool ExistsByUsername(string username);

    Task<IEnumerable<User>> FindByOrganizationIdAsync(Guid organizationId);

    Task<IEnumerable<User>> FindByRoleAsync(Guid organizationId, Roles role);
}
