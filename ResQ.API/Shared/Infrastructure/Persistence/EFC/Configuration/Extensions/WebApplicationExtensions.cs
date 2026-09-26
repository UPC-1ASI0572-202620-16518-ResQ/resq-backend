namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class WebApplicationExtensions
{
    public static void UseDatabaseCreationAssurance(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
}