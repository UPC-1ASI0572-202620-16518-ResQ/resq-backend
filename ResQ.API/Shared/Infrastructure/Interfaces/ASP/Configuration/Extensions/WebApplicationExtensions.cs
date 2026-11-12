using Microsoft.AspNetCore.Diagnostics;

namespace ResQ.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
///     Extensiones del pipeline HTTP compartidas por todos los bounded contexts.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    ///     Captura cualquier excepción no controlada, la registra en el log y responde
    ///     500 con el cuerpo <c>{ message }</c>, sin exponer el stack trace al cliente.
    /// </summary>
    /// <param name="app">La aplicación web.</param>
    public static void UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                if (exception != null)
                {
                    var logger = context.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("GlobalExceptionHandler");
                    logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                        context.Request.Method, context.Request.Path);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "An unexpected error occurred while processing the request."
                });
            });
        });
    }
}
