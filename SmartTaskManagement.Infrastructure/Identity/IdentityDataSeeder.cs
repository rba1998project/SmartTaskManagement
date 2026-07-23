using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Authorization;

namespace SmartTaskManagement.Infrastructure.Identity;

/// <summary>
/// Idempotent startup seeding of the three roles, a default admin user, and demo users.
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
        await SeedDemoUsersAsync();
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

    private async Task SeedDemoUsersAsync()
    {
        var enableDemoUsers = configuration.GetValue<bool?>("Seed:EnableDemoUsers");
        if (enableDemoUsers is not true)
        {
            logger.LogInformation("Demo user seed skipped: Seed:EnableDemoUsers is not true.");
            return;
        }

        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Demo user seed skipped: Seed:AdminPassword not set.");
            return;
        }

        var demoUsers = new[]
        {
            new { Email = "demo.PM1@smarttask.local", FullName = "Demo Project Manager 1", Role = RoleNames.ProjectManager },
            new { Email = "demo.PM2@smarttask.local", FullName = "Demo Project Manager 2", Role = RoleNames.ProjectManager },
            new { Email = "demo.TM1@smarttask.local", FullName = "Demo Team Member 1", Role = RoleNames.TeamMember },
            new { Email = "demo.TM2@smarttask.local", FullName = "Demo Team Member 2", Role = RoleNames.TeamMember },
            new { Email = "demo.TM3@smarttask.local", FullName = "Demo Team Member 3", Role = RoleNames.TeamMember }
        };

        foreach (var user in demoUsers)
        {
            if (await userManager.FindByEmailAsync(user.Email) is not null)
                continue;

            var appUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                EmailConfirmed = true,
                FullName = user.FullName
            };

            var created = await userManager.CreateAsync(appUser, password);
            if (!created.Succeeded)
            {
                logger.LogError("Failed to seed demo user {Email}: {Errors}",
                    user.Email, string.Join("; ", created.Errors.Select(e => e.Description)));
                continue;
            }

            var roleAssigned = await userManager.AddToRoleAsync(appUser, user.Role);
            if (roleAssigned.Succeeded)
                logger.LogInformation("Seeded demo user {Email} with role {Role}.", user.Email, user.Role);
            else
                logger.LogError("Seeded demo user {Email} but failed to assign role {Role}: {Errors}",
                    user.Email, user.Role, string.Join("; ", roleAssigned.Errors.Select(e => e.Description)));
        }
    }
}
