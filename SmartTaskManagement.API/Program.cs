using SmartTaskManagement.API.Extensions;
using SmartTaskManagement.Infrastructure;
using SmartTaskManagement.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();

builder.Services.AddInfrastructure(builder.Configuration); // call to the Infrastructure layer to register services like DbContext
builder.Services.AddApiServices(builder.Configuration); // extension: call to the API layer to register services like controllers, swagger, cors, health checks, and rate limiting

var app = builder.Build();

// Seed roles and the default admin user (idempotent).
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    await seeder.SeedAsync();
}

app.UseApiPipeline(); // extension: call to the API layer to configure the middleware pipeline
app.Run();
