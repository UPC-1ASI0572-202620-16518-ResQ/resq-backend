using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.IAM.Domain.Repositories;

public interface IOrganizationRepository : IBaseRepository<Organization>
{
    Task<Organization?> FindByOrganizationIdAsync(Guid organizationId);
}
