using SmartTaskManagement.API.Extensions;
using SmartTaskManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();

builder.Services.AddInfrastructure(builder.Configuration); // call to the Infrastructure layer to register services like DbContext
builder.Services.AddApiServices(builder.Configuration); // extension: call to the API layer to register services like controllers, swagger, cors, health checks, and rate limiting

var app = builder.Build();

app.UseApiPipeline(); // extension: call to the API layer to configure the middleware pipeline
app.Run();
