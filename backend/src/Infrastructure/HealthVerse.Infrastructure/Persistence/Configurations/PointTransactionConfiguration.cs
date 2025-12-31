using HealthVerse.Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for PointTransaction entity.
/// This keeps the domain entity clean (POCO) and moves all persistence concerns to Infrastructure.
/// </summary>
public sealed class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        // Table mapping
        builder.ToTable("PointTransactions", "gamification");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Value Object =====
        builder.OwnsOne(x => x.IdempotencyKey, idempotencyKey =>
        {
            idempotencyKey.Property(k => k.Value)
                .HasColumnName("IdempotencyKey")
                .HasMaxLength(255)
                .IsRequired();

            // Unique index for idempotency
            idempotencyKey.HasIndex(k => k.Value)
                .IsUnique();
        });

        // ===== Core Properties =====
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SourceIdText)
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // ===== Index for efficient queries =====
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}
