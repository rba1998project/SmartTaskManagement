using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Infrastructure.Identity;

namespace SmartTaskManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RefreshToken"/> entity. Stored in the
/// <c>RefreshTokens</c> table, keyed to the Identity user by <c>UserId</c>.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    // SHA-256 hashes are 64 hex chars / 44 base64 chars — 128 leaves comfortable headroom.
    private const int TokenHashMaxLength = 128;

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(TokenHashMaxLength);

        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();

        // Looked up by hash on every refresh — index it. Unique so a hash maps to one token.
        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        // Enables "revoke all tokens for a user" and per-user queries.
        builder.HasIndex(rt => rt.UserId);

        // FK to the Identity users table by id, without a navigation on the pure Domain entity.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
