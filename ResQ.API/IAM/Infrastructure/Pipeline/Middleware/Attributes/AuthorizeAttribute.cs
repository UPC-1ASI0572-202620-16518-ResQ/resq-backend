using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;

/// <summary>
///     Indicates that the decorated controller or action requires an authenticated user and,
///     optionally, one of the given roles.
/// </summary>
/// <remarks>
///     The filter checks <c>HttpContext.Items["User"]</c> to determine whether a user was set
///     by the request authorization middleware. Without a user it returns 401 Unauthorized.
///     When roles are given and the user has none of them it returns 403 Forbidden with a
///     <c>{ message }</c> body. Endpoints annotated with <see cref="AllowAnonymousAttribute" /> are skipped.
///     Usage: <c>[Authorize]</c> on the controller and <c>[Authorize(Roles.Admin)]</c> on restricted actions.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthorizeAttribute(params Roles[] roles) : Attribute, IAuthorizationFilter
{
    /// <summary>
    ///     Roles allowed to call the endpoint. Empty means any authenticated user.
    /// </summary>
    public IReadOnlyCollection<Roles> AllowedRoles { get; } = roles;

    /// <summary>
    ///     Called by the MVC framework to perform authorization.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous) return;

        var user = (User?)context.HttpContext.Items["User"];

        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (AllowedRoles.Count > 0 && !AllowedRoles.Contains(user.Role))
        {
            context.Result = new ObjectResult(new
            {
                message = $"This action requires one of these roles: {string.Join(", ", AllowedRoles.Select(r => r.ToString().ToLowerInvariant()))}."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
