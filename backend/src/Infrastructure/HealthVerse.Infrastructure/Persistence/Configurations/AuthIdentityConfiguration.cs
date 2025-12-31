using HealthVerse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for AuthIdentity entity.
/// </summary>
public sealed class AuthIdentityConfiguration : IEntityTypeConfiguration<AuthIdentity>
{
    public void Configure(EntityTypeBuilder<AuthIdentity> builder)
    {
        // Table mapping
        builder.ToTable("AuthIdentities", "identity");

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.FirebaseUid)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ProviderEmail)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastLoginAt)
            .IsRequired();

        // ===== Indexes =====
        // Firebase UID unique olmalı
        builder.HasIndex(x => x.FirebaseUid)
            .IsUnique()
            .HasDatabaseName("UX_AuthIdentities_FirebaseUid");

        // Kullanıcının auth provider'larını bulmak için
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_AuthIdentities_User");

        // Provider + Email kombinasyonu
        builder.HasIndex(x => new { x.Provider, x.ProviderEmail })
            .HasDatabaseName("IX_AuthIdentities_ProviderEmail");
    }
}
