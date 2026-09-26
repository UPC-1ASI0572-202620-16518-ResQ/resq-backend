using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Components;

namespace ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Extensions;

/// <summary>
///     Extension methods to register the custom request authorization middleware.
/// </summary>
public static class RequestAuthorizationMiddlewareExtensions
{
    /// <summary>
    ///     Registers the <see cref="RequestAuthorizationMiddleware" /> into the application's request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder" /> to configure.</param>
    /// <returns>The original <paramref name="builder" /> for chaining.</returns>
    public static IApplicationBuilder UseRequestAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestAuthorizationMiddleware>();
    }
}
