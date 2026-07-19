using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartTaskManagement.Application.Authentication;

namespace SmartTaskManagement.Application;

/// <summary>
/// Registers Application-layer services: the auth orchestration and the FluentValidation
/// validators. The abstractions these depend on are supplied by Infrastructure (see 2E).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
