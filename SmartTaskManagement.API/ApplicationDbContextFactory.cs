using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SmartTaskManagement.Infrastructure.Persistence;

namespace SmartTaskManagement.API;

/// <summary>
/// Design-time factory used by the EF Core CLI (migrations, <c>database update</c>) to build an
/// <see cref="ApplicationDbContext"/> WITHOUT booting the full web host. This decouples the
/// migration workflow from application-layer DI: feature services registered in later phases
/// (e.g. <c>ProjectService</c> depending on <c>ICurrentUserService</c>, which is only wired at
/// runtime) would otherwise fail design-time DI validation before their dependencies exist.
/// Not used at runtime — the runtime context is registered via <c>AddInfrastructure</c>.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Mirror the runtime configuration sources so the connection string resolves from the
        // same place (User Secrets locally); appsettings/env vars are included for completeness.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("SmartTaskConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'SmartTaskConnection' was not found. " +
                "Set it in API User Secrets under 'ConnectionStrings:SmartTaskConnection'.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
