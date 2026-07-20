using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Infrastructure.Authentication;
using SmartTaskManagement.Infrastructure.Identity;
using SmartTaskManagement.Infrastructure.Persistence;

namespace SmartTaskManagement.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services (EF Core, Identity, auth services) with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure( this IServiceCollection services, IConfiguration configuration)
    {

        // Register the ApplicationDbContext with the DI container, using the connection string from configuration.
        var connectionString = configuration.GetConnectionString("SmartTaskConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'SmartTaskConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Identity services with the DI container, using ApplicationUser and ApplicationRole.
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Register JWT options with the DI container, binding them to the "Jwt" section of the configuration.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Register the JWT signing key with the DI container, throwing an exception if it is not found in the configuration.
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "JWT signing key 'Jwt:SigningKey' was not found.");

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator>(sp =>
            new JwtTokenGenerator(sp.GetRequiredService<IOptions<JwtOptions>>(), signingKey));
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Register the IdentityDataSeeder with the DI container, which is responsible for seeding initial identity data.
        services.AddScoped<IdentityDataSeeder>();

        return services;
    }
}
