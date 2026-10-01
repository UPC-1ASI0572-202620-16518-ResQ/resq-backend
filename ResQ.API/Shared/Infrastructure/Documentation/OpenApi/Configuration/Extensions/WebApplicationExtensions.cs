namespace ResQ.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;

public static class WebApplicationExtensions
{
    public static void UseOpenApiDocumentation(this WebApplication app)
    {
        // Mantiene disponible el OpenAPI nativo de ASP.NET Core.
        app.MapOpenApi();

        // Genera el documento OpenAPI mediante Swashbuckle.
        app.UseSwagger();

        // Configura Swagger UI para consumir la definición de Swashbuckle.
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "ResQ API v1"
            );
        });
    }
}