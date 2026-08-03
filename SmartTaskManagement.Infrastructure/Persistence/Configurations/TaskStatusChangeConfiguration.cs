using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for immutable task status audit records.</summary>
public sealed class TaskStatusChangeConfiguration : IEntityTypeConfiguration<TaskStatusChange>
{
    public void Configure(EntityTypeBuilder<TaskStatusChange> builder)
    {
        builder.ToTable("TaskStatusChanges");
        builder.HasKey(change => change.Id);

        builder.Property(change => change.FromStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(change => change.ToStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(change => change.Comment).HasMaxLength(1000);
        builder.Property(change => change.ChangedByUserId).IsRequired();
        builder.Property(change => change.ChangedByDisplayName)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(change => change.ChangedAt).IsRequired();

        builder.HasOne<TaskItem>()
            .WithMany()
            .HasForeignKey(change => change.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(change => new { change.TaskId, change.ChangedAt });
    }
}
