using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="TaskItem"/> entity.
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    private const int TitleMaxLength = 200;
    private const int DescriptionMaxLength = 2000;

    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(DescriptionMaxLength);

        // Store enums as their string names so the column stays readable and stable if new
        // members are inserted (ordinal values would shift). IMPORTANT: this conversion only
        // affects the database column. The API serializes these enums as numeric values by
        // default (System.Text.Json), matching the frontend's numeric TypeScript enums. Do NOT
        // add JsonStringEnumConverter without also changing the frontend to string enums.
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.DueDate);
        builder.Property(t => t.AssignedToUserId);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        // Tasks are listed by project; deleting a project removes its tasks so none are orphaned.
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Listing tasks within a project filters on ProjectId.
        builder.HasIndex(t => t.ProjectId);

        // A Team Member's "my tasks" lookups filter on AssignedToUserId.
        builder.HasIndex(t => t.AssignedToUserId);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
