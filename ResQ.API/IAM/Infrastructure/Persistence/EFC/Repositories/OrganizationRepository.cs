using Microsoft.EntityFrameworkCore;
using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class OrganizationRepository(AppDbContext context)
    : IOrganizationRepository
{
    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await context.Set<Organization>().AddAsync(organization, cancellationToken);
    }

    public async Task<Organization?> FindByIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await context.Set<Organization>()
            .FirstOrDefaultAsync(
                organization => organization.Id == organizationId,
                cancellationToken);
    }
}