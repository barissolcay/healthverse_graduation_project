using HealthVerse.Competition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for LeagueConfig entity.
/// This is a catalog/lookup table with string PK (TierName).
/// </summary>
public sealed class LeagueConfigConfiguration : IEntityTypeConfiguration<LeagueConfig>
{
    public void Configure(EntityTypeBuilder<LeagueConfig> builder)
    {
        // Table mapping
        builder.ToTable("LeagueConfigs", "competition");

        // Primary Key (string, not UUID!)
        builder.HasKey(x => x.TierName);

        // ===== Properties =====
        builder.Property(x => x.TierName)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TierOrder)
            .IsRequired();

        builder.Property(x => x.PromotePercentage)
            .IsRequired();

        builder.Property(x => x.DemotePercentage)
            .IsRequired();

        builder.Property(x => x.MinRoomSize)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(x => x.MaxRoomSize)
            .IsRequired()
            .HasDefaultValue(20);

        // ===== Indexes =====
        builder.HasIndex(x => x.TierOrder)
            .IsUnique();

        // ===== Seed Data (7 Turkish tiers) =====
        builder.HasData(
            LeagueConfig.Create("ISINMA",       1, 30,  0, 10, 20),
            LeagueConfig.Create("ANTRENMAN",    2, 20, 10, 10, 20),
            LeagueConfig.Create("TEMPO",        3, 20, 10, 10, 20),
            LeagueConfig.Create("FORM",         4, 20, 10, 10, 20),
            LeagueConfig.Create("KONDISYON",    5, 20, 10, 10, 20),
            LeagueConfig.Create("DAYANIKLILIK", 6, 20, 10, 10, 20),
            LeagueConfig.Create("SAMPIYON",     7,  0, 20, 10, 20)
        );
    }
}
