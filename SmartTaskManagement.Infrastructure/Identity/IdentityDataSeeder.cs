using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Authorization;

namespace SmartTaskManagement.Infrastructure.Identity;

/// <summary>
/// Idempotent startup seeding of the three roles and a single default admin user.
/// Safe to run on every startup — it only creates what is missing.
/// </summary>
public class IdentityDataSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<IdentityDataSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new ApplicationRole(roleName));
            if (result.Succeeded)
                logger.LogInformation("Seeded role {Role}.", roleName);
            else
                logger.LogError("Failed to seed role {Role}: {Errors}",
                    roleName, string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    // Grants each role its default permissions as role claims. Only adds claims that are
    // missing, so it is safe to re-run and picks up new permissions on later startups.
    private async Task SeedRolePermissionsAsync()
    {
        foreach (var mapping in Permissions.DefaultRolePermissions)
        {
            var role = await roleManager.FindByNameAsync(mapping.Key);
            if (role is null)
                continue;

            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in mapping.Value)
            {
                var alreadyGranted = existingClaims.Any(c =>
                    c.Type == Permissions.ClaimType && c.Value == permission);
                if (alreadyGranted)
                    continue;

                var claim = new System.Security.Claims.Claim(Permissions.ClaimType, permission);
                var result = await roleManager.AddClaimAsync(role, claim);
                if (result.Succeeded)
                    logger.LogInformation("Seeded permission {Permission} for role {Role}.", permission, mapping.Key);
                else
                    logger.LogError("Failed to seed permission {Permission} for role {Role}: {Errors}",
                        permission, mapping.Key, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Admin seed skipped: Seed:AdminEmail (config) and/or Seed:AdminPassword (User Secrets) not set.");
            return;
        }

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "System Administrator"
        };

        var created = await userManager.CreateAsync(admin, password);
        if (!created.Succeeded)
        {
            logger.LogError("Failed to seed admin user: {Errors}",
                string.Join("; ", created.Errors.Select(e => e.Description)));
            return;
        }

        var roleAssigned = await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        if (roleAssigned.Succeeded)
            logger.LogInformation("Seeded admin user {Email}.", email);
        else
            logger.LogError("Seeded admin user but failed to assign Admin role: {Errors}",
                string.Join("; ", roleAssigned.Errors.Select(e => e.Description)));
    }
}
