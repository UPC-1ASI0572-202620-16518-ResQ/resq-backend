using Microsoft.AspNetCore.Mvc;

namespace ResQ.API.Monitoring.Interfaces.REST;

/// <summary>
/// Helpers used by Monitoring controllers to read the request context and translate domain exceptions.
/// </summary>
internal static class MonitoringRequestContext
{
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