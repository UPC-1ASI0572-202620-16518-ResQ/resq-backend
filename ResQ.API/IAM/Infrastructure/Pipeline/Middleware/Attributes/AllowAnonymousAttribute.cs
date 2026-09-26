namespace ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;

/// <summary>
///     Marks an action or controller to bypass the custom authorization middleware.
/// </summary>
/// <remarks>
///     When present, the
///     <see cref="ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Components.RequestAuthorizationMiddleware" />
///     will skip authorization for the decorated endpoint.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{
}
