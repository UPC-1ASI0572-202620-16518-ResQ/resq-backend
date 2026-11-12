using Microsoft.EntityFrameworkCore;
using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Repositories;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class OrganizationRepository(AppDbContext context)
    : BaseRepository<Organization>(context), IOrganizationRepository
{
    public async Task<Organization?> FindByOrganizationIdAsync(Guid organizationId)
    {
        return await Context.Set<Organization>().FirstOrDefaultAsync(o => o.Id == organizationId);
    }
}
