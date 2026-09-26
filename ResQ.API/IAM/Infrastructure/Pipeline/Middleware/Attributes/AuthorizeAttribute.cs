using ResQ.API.IAM.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;

/// <summary>
///     Indicates that the decorated controller or action requires an authenticated user.
/// </summary>
/// <remarks>
///     The filter checks <c>HttpContext.Items["User"]</c> to determine whether a user was set
///     by the request authorization middleware. If no user is present it sets a <see cref="UnauthorizedResult" />.
///     Endpoints annotated with <see cref="AllowAnonymousAttribute" /> are skipped.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    ///     Called by the MVC framework to perform authorization.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous)
        {
            Console.WriteLine("Skipping authorization");
            return;
        }

        var user = (User?)context.HttpContext.Items["User"];

        if (user == null) context.Result = new UnauthorizedResult();
    }
}
