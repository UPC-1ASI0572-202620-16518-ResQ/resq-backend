using ResQ.API.IAM.Domain.Model.Aggregates;

namespace ResQ.API.IAM.Domain.Repositories;

public interface IOrganizationRepository
{
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);

    Task<Organization?> FindByIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}