using Serilog;
using SmartTaskManagement.API.Authentication;
using SmartTaskManagement.API.Middleware;
using SmartTaskManagement.Application;
using SmartTaskManagement.Application.Abstractions;

namespace SmartTaskManagement.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Extension method to register API services like controllers, swagger, cors, health checks, and rate limiting.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        // Exposes the authenticated caller to the Application layer from the request's JWT claims.
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddApplication();
        services.AddApiAuthentication(configuration);
        services.AddApiSwagger();
        services.AddApiCors(configuration);
        services.AddApiHealthChecks();
        services.AddApiRateLimiting();

        return services;
    }

    /// <summary>
    /// Extension method to configure the API middleware pipeline.
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseSerilogRequestLogging();

        // Enable Swagger only in development environment. We don't want to expose our API documentation in production for security reasons.
        if (app.Environment.IsDevelopment()) 
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Enforce HTTPS redirection to ensure secure communication between the client and server.
        app.UseHttpsRedirection();

        // Enable CORS to allow cross-origin requests from the specified origins in the configuration. see /CorsExtensions.cs for more details.
        app.UseCors(CorsExtensions.CorsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapControllers();

        app.MapApiHealthChecks();

        return app;
    }
}
