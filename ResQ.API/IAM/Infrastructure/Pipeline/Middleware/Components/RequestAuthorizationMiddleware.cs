using ResQ.API.IAM.Application.Internal.OutboundServices;
using ResQ.API.IAM.Domain.Model.Queries;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;

namespace ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Middleware that authorizes incoming HTTP requests by validating a JWT token in the Authorization header.
/// </summary>
/// <remarks>
///     If a valid token is present the corresponding
///     <see cref="ResQ.API.IAM.Domain.Model.Aggregates.User" /> is loaded
///     and placed into <c>HttpContext.Items["User"]</c>. Endpoints decorated with <see cref="AllowAnonymousAttribute" />
///     are skipped.
/// </remarks>
public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    /// <summary>
    ///     Invoked by the ASP.NET Core pipeline to authorize the request.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext" />.</param>
    /// <param name="userQueryService">Service used to load users by id.</param>
    /// <param name="tokenService">Service used to validate JWT tokens.</param>
    /// <param name="configuration"> Service used to configure the initial parameters.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    /// <exception cref="Exception">Thrown when the token is missing or invalid.</exception>
    public async Task InvokeAsync(HttpContext context, IUserQueryService userQueryService, ITokenService tokenService, IConfiguration configuration)
    {
        Console.WriteLine("Entering InvokeAsync");

        // Swagger and OpenAPI documentation do not require authentication
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/openapi"))
        {
            await next(context);
            return;
        }

        var endpoint = context.GetEndpoint();

        var allowAnonymous = endpoint?.Metadata.Any(m => m.GetType() == typeof(AllowAnonymousAttribute)) ?? false;

        Console.WriteLine($"Allow Anonymous is {allowAnonymous}");

        if (allowAnonymous)
        {
            Console.WriteLine("Skipping authorization");
            await next(context);
            return;
        }

        Console.WriteLine("Entering authorization");

        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?
            .Split(" ")
            .Last();

        if (token == null) throw new Exception("Null or invalid token");

        var userId = await tokenService.ValidateToken(token);

        if (userId == null) throw new Exception("Invalid token");

        var getUserByIdQuery = new GetUserByIdQuery(userId.Value);

        var user = await userQueryService.Handle(getUserByIdQuery);

        if (user == null) throw new Exception("User not found");

        context.Items["User"] = user;

        // Temporary single-organization context for development
        var organizationIdValue = configuration["DevelopmentSettings:OrganizationId"];

        if (!Guid.TryParse(organizationIdValue, out var organizationId))
        {
            throw new Exception("Development organization id is not configured correctly.");
        }

        context.Items["OrganizationId"] = organizationId;

        Console.WriteLine("Successful authorization. Updating Context...");

        await next(context);
    }
}
