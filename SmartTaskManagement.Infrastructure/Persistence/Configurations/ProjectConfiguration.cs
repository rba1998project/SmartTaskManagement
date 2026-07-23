using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Project"/> entity. 
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    private const int NameMaxLength = 200;
    private const int DescriptionMaxLength = 2000;

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        // Ownership lookups ("projects owned by this user") filter on CreatedByUserId.
        builder.HasIndex(p => p.CreatedByUserId);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
