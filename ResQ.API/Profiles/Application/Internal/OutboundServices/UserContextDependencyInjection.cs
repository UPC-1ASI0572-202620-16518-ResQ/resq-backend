using ResQ.API.Profiles.Application.Internal.CommandServices;
using ResQ.API.Profiles.Application.Internal.QueryServices;
using ResQ.API.Profiles.Domain.Repositories;
using ResQ.API.Profiles.Domain.Services;
using ResQ.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.API.Profiles.Application.Internal.OutboundServices;

public static class UserContextDependencyInjection
{
    public static IServiceCollection AddUserContextServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        // Command Services
        services.AddScoped<IUserProfileCommandService, UserProfileCommandService>();

        // Query Services
        services.AddScoped<IUserProfileQueryService, UserProfileQueryService>();

        // ACL
        services.AddScoped<ResQ.API.Profiles.Interfaces.ACL.IProfilesContextFacade, ResQ.API.Profiles.Interfaces.ACL.ProfilesContextFacade>();

        return services;
    }
}
