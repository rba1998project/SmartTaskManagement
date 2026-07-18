using System.Text.Json;
using SmartTaskManagement.API.Common;

namespace SmartTaskManagement.API.Middleware;

/// <summary>
/// Catches unhandled exceptions from the request pipeline and returns a
/// consistent <see cref="ApiResponse"/> error payload instead of leaking
/// stack traces to clients. Details are written to the log, not the response.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var message = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred.";

            var response = ApiResponse.Fail("An unexpected error occurred.", new[] { message });

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // name the properties in camelCase format
            });

            await context.Response.WriteAsync(json);
        }
    }
}
