using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthVerse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for Duel entity.
/// Includes 11 check constraints as per DB schema.
/// </summary>
public sealed class DuelConfiguration : IEntityTypeConfiguration<Duel>
{
    public void Configure(EntityTypeBuilder<Duel> builder)
    {
        // Table mapping with extensive check constraints
        builder.ToTable("Duels", "social", t =>
        {
            // Self-duel prevention
            t.HasCheckConstraint("CHK_Duels_NoSelf", 
                "\"ChallengerId\" <> \"OpponentId\"");

            // Valid status values
            t.HasCheckConstraint("CHK_Duels_Status", 
                "\"Status\" IN ('WAITING','ACTIVE','FINISHED','REJECTED','EXPIRED')");

            // Valid result values
            t.HasCheckConstraint("CHK_Duels_Result", 
                "\"Result\" IS NULL OR \"Result\" IN ('CHALLENGER_WIN','OPPONENT_WIN','BOTH_WIN','BOTH_LOSE')");

            // Time order constraint
            t.HasCheckConstraint("CHK_Duels_TimeOrder", 
                "\"StartDate\" IS NULL OR \"EndDate\" IS NULL OR \"EndDate\" > \"StartDate\"");

            // ACTIVE requires StartDate
            t.HasCheckConstraint("CHK_Duels_ActiveHasStart", 
                "\"Status\" <> 'ACTIVE' OR \"StartDate\" IS NOT NULL");

            // FINISHED requires StartDate
            t.HasCheckConstraint("CHK_Duels_FinishedHasStart", 
                "\"Status\" <> 'FINISHED' OR \"StartDate\" IS NOT NULL");

            // ACTIVE requires EndDate
            t.HasCheckConstraint("CHK_Duels_ActiveHasEnd", 
                "\"Status\" <> 'ACTIVE' OR \"EndDate\" IS NOT NULL");

            // WAITING has no dates or result
            t.HasCheckConstraint("CHK_Duels_WaitingHasNoDates", 
                "\"Status\" <> 'WAITING' OR (\"StartDate\" IS NULL AND \"EndDate\" IS NULL AND \"Result\" IS NULL)");

            // REJECTED has no dates or result
            t.HasCheckConstraint("CHK_Duels_RejectedHasNoDates", 
                "\"Status\" <> 'REJECTED' OR (\"StartDate\" IS NULL AND \"EndDate\" IS NULL AND \"Result\" IS NULL)");

            // EXPIRED has no dates or result
            t.HasCheckConstraint("CHK_Duels_ExpiredHasNoDates", 
                "\"Status\" <> 'EXPIRED' OR (\"StartDate\" IS NULL AND \"EndDate\" IS NULL AND \"Result\" IS NULL)");

            // Result only when FINISHED
            t.HasCheckConstraint("CHK_Duels_ResultOnlyWhenFinished", 
                "\"Result\" IS NULL OR \"Status\" = 'FINISHED'");

            // FINISHED requires EndDate and Result
            t.HasCheckConstraint("CHK_Duels_FinishedHasEndAndResult", 
                "\"Status\" <> 'FINISHED' OR (\"EndDate\" IS NOT NULL AND \"Result\" IS NOT NULL)");

            // Duration days between 1 and 7
            t.HasCheckConstraint("CHK_Duels_DurationDays", 
                "\"DurationDays\" BETWEEN 1 AND 7");

            // TargetValue must be positive
            t.HasCheckConstraint("CHK_Duels_TargetValue", 
                "\"TargetValue\" > 0");

            // Scores must be non-negative
            t.HasCheckConstraint("CHK_Duels_Scores", 
                "\"ChallengerScore\" >= 0 AND \"OpponentScore\" >= 0");
        });

        // Primary Key
        builder.HasKey(x => x.Id);

        // ===== Properties =====
        builder.Property(x => x.ChallengerId)
            .IsRequired();

        builder.Property(x => x.OpponentId)
            .IsRequired();

        builder.Property(x => x.ActivityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TargetMetric)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TargetValue)
            .IsRequired();

        builder.Property(x => x.DurationDays)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("WAITING");

        builder.Property(x => x.ChallengerScore)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.OpponentScore)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Result)
            .HasMaxLength(20);

        builder.Property(x => x.StartDate);
        builder.Property(x => x.EndDate);
        builder.Property(x => x.ChallengerLastPokeAt);
        builder.Property(x => x.OpponentLastPokeAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // ===== Indexes =====
        // Partial unique index: Aynı ikili arasında tek WAITING/ACTIVE düello
        // NOT: EF Core partial unique index'i SQL ile oluşturmak gerekecek (migration'da)
        // Şimdilik normal index oluşturuyoruz, partial index migration'da eklenecek
        builder.HasIndex(x => new { x.ChallengerId, x.OpponentId })
            .HasDatabaseName("IX_Duels_Pair");

        // Bekleyen düelloları expire etmek için
        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("IX_Duels_Expire_WaitingCreatedAt");

        // Aktivite tipine göre sorgular
        builder.HasIndex(x => x.ActivityType)
            .HasDatabaseName("IX_Duels_ActivityType");

        // Kullanıcının düellolarını bulmak için
        builder.HasIndex(x => x.ChallengerId)
            .HasDatabaseName("IX_Duels_Challenger");

        builder.HasIndex(x => x.OpponentId)
            .HasDatabaseName("IX_Duels_Opponent");
    }
}
