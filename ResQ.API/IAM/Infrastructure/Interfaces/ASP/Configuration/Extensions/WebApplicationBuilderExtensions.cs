using ResQ.API.IAM.Application.ACL.Services;
using ResQ.API.IAM.Application.Internal.CommandServices;
using ResQ.API.IAM.Application.Internal.OutboundServices;
using ResQ.API.IAM.Application.Internal.QueryServices;
using ResQ.API.IAM.Domain.Repositories;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using ResQ.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using ResQ.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using ResQ.API.IAM.Infrastructure.Tokens.JWT.Services;
using ResQ.API.IAM.Interfaces.ACL;

namespace ResQ.API.IAM.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
///     Extension methods for configuring IAM context services in a WebApplicationBuilder.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds the IAM context services to the WebApplicationBuilder.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure.</param>
    public static void AddIamContextServices(this WebApplicationBuilder builder)
    {
        // IAM Bounded Context Injection Configuration

        // TokenSettings Configuration
        builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserCommandService, UserCommandService>();
        builder.Services.AddScoped<IUserQueryService, UserQueryService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IHashingService, HashingService>();
        builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();
    }
}
