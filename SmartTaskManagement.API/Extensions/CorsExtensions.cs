namespace SmartTaskManagement.API.Extensions;

public static class CorsExtensions
{
    public const string CorsPolicyName = "AngularDevClient";

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        return services;
    }
}
