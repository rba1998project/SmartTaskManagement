using Microsoft.EntityFrameworkCore;

namespace SmartTaskManagement.Infrastructure.Persistence;

/// <summary>
/// Application database context. Entity configurations are added in later phases
/// as domain entities are introduced.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
