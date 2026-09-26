using System.Text.Json;
using System.Text.Json.Serialization;
using ResQ.API.IAM.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using ResQ.API.Shared.Infrastructure.Documentation.OpenApi.Configuration.Extensions;
using ResQ.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using ResQ.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using ResQ.API.Shared.Infrastructure.Mediator.Cortex.Configuration.Extensions;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod().AllowAnyHeader());
});
// ------------------------------------

// Add services to the container.

builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

// Add Database Services
builder.AddDatabaseServices();

// Open API Configuration (Swagger)
builder.AddOpenApiDocumentationServices();

// Shared Context Services
builder.AddSharedContextServices();

// IAM Bounded Context Services
builder.AddIamContextServices();

// Mediator Configuration
builder.AddCortexConfigurationServices();

var app = builder.Build();

// Verify if the database exists and create it if it doesn't
app.UseDatabaseCreationAssurance();

// Apply CORS Policy
app.UseCors("AllowAllPolicy");

// Configure the HTTP request pipeline.
app.UseOpenApiDocumentation();

//app.UseHttpsRedirection();

app.UseAuthorization();
app.UseRequestAuthorization();
app.MapControllers();
app.Run();