using Microsoft.AspNetCore.Mvc;
using ResQ.API.IAM.Domain.Model.Aggregates;

namespace ResQ.API.Alert_Management.Interfaces.REST;

/// <summary>
/// Helpers shared by the Alert Management controllers to read the request context
/// and translate domain exceptions into HTTP responses.
/// </summary>
internal static class AlertRequestContext
{
    /// <summary>
    /// Reads the organization placed in HttpContext.Items by RequestAuthorizationMiddleware.
    /// </summary>
    public static bool TryGetOrganizationId(this HttpContext httpContext, out Guid organizationId)
    {
        organizationId = Guid.Empty;

        switch (httpContext.Items["OrganizationId"])
        {
            case Guid id when id != Guid.Empty:
                organizationId = id;
                return true;
            case string value when Guid.TryParse(value, out var parsed) && parsed != Guid.Empty:
                organizationId = parsed;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Returns the identifier of the authenticated user, if any.
    /// </summary>
    public static string? GetCurrentUserId(this HttpContext httpContext)
    {
        return httpContext.Items["User"] is User user ? user.Id.ToString() : null;
    }

    /// <summary>
    /// Runs an action and maps ArgumentException to 400, KeyNotFoundException to 404
    /// and InvalidOperationException to 409, using the { message } body convention.
    /// </summary>
    public static async Task<IActionResult> ExecuteAsync(this ControllerBase controller, Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException e)
        {
            return controller.BadRequest(new { message = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            return controller.NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return controller.Conflict(new { message = e.Message });
        }
    }
}
