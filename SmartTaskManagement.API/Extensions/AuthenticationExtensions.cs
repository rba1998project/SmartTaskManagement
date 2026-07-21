using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Infrastructure.Authentication;

namespace SmartTaskManagement.API.Extensions;

/// <summary>
/// Registers JWT bearer authentication and a global fallback authorization policy that
/// requires an authenticated user. Endpoints opt out with <c>[AllowAnonymous]</c>.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT settings ('Jwt' section) were not found.");

        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key 'Jwt:SigningKey' was not found.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // One policy per permission: the policy name is the permission key, and it requires a
            // matching permission claim (carried in the JWT). Endpoints opt in with [Authorize(Policy = ...)].
            foreach (var permission in Permissions.AllPermissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireClaim(Permissions.ClaimType, permission));
            }
        });

        return services;
    }
}
