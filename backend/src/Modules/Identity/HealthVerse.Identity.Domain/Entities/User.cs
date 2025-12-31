using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.Entities;

/// <summary>
/// User aggregate root representing the application profile.
/// This is a rich domain model with behavior, not an anemic model.
/// </summary>
public sealed class User : AggregateRoot
{
    // ===== Core Profile =====
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public int AvatarId { get; private set; } = 1;
    public string? City { get; private set; }
    public string? Bio { get; private set; }

    // ===== Gamification Counters (Cached) =====
    public long TotalPoints { get; private set; }
    public int FreezeInventory { get; private set; }
    public int StreakCount { get; private set; }
    public DateOnly? LastStreakDate { get; private set; }
    public int LongestStreakCount { get; private set; }

    // ===== Statistics Counters =====
    public int TotalTasksCompleted { get; private set; }
    public int TotalDuels { get; private set; }
    public int TotalDuelsWon { get; private set; }
    public int TotalGlobalMissions { get; private set; }

    // ===== Social Counters =====
    public int FollowingCount { get; private set; }
    public int FollowersCount { get; private set; }

    // ===== League =====
    public string CurrentTier { get; private set; } = "ISINMA";
    public string? SelectedTitleId { get; private set; }

    // ===== Health Permission =====
    public bool HealthPermissionGranted { get; private set; }
    public DateTimeOffset? HealthPermissionGrantedAt { get; private set; }

    // ===== Metadata (UI preferences, experimental features) =====
    public string? Metadata { get; private set; }

    private User() { }

    public static User Create(Username username, Email email, int avatarId = 1)
    {
        if (avatarId <= 0)
            throw new DomainException("User.InvalidAvatarId", "AvatarId must be positive.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            AvatarId = avatarId,
        };

        user.AddDomainEvent(new Events.UserCreatedEvent(user.Id, username.Value, email.Value));
        return user;
    }

    // ===== Profile Methods =====

    public void UpdateAvatar(int avatarId)
    {
        if (avatarId <= 0)
            throw new DomainException("User.InvalidAvatarId", "AvatarId must be positive.");

        AvatarId = avatarId;
        SetUpdatedAt();
    }

    public void UpdateCity(string? city)
    {
        City = city?.Trim();
        SetUpdatedAt();
    }

    public void UpdateBio(string? bio)
    {
        if (bio?.Length > 150)
            throw new DomainException("User.BioTooLong", "Bio cannot exceed 150 characters.");

        Bio = bio?.Trim();
        SetUpdatedAt();
    }

    public void SelectTitle(string? titleId)
    {
        SelectedTitleId = titleId;
        SetUpdatedAt();
    }

    // ===== Health Permission =====

    public void GrantHealthPermission(DateTimeOffset grantedAt)
    {
        if (HealthPermissionGranted)
            return;

        HealthPermissionGranted = true;
        HealthPermissionGrantedAt = grantedAt;
        SetUpdatedAt();

        AddDomainEvent(new Events.HealthPermissionGrantedEvent(Id, grantedAt));
    }

    public void RevokeHealthPermission()
    {
        if (!HealthPermissionGranted)
            return;

        HealthPermissionGranted = false;
        HealthPermissionGrantedAt = null;
        SetUpdatedAt();
    }

    // ===== Points (Updated via PointTransactions trigger) =====

    public void AddPoints(long amount)
    {
        if (TotalPoints + amount < 0)
            throw new DomainException("User.NegativePoints", "Total points cannot be negative.");

        TotalPoints += amount;
        SetUpdatedAt();
    }

    // ===== Freeze Inventory =====

    public void AddFreezeRight(int count = 1)
    {
        if (count <= 0)
            throw new DomainException("User.InvalidFreezeCount", "Freeze count must be positive.");

        FreezeInventory += count;
        SetUpdatedAt();
    }

    public bool UseFreeze()
    {
        if (FreezeInventory <= 0)
            return false;

        FreezeInventory--;
        SetUpdatedAt();
        return true;
    }

    // ===== Streak =====

    public void UpdateStreak(int newStreakCount, DateOnly lastDate)
    {
        if (newStreakCount < 0)
            throw new DomainException("User.InvalidStreakCount", "Streak count cannot be negative.");

        StreakCount = newStreakCount;
        LastStreakDate = lastDate;

        if (newStreakCount > LongestStreakCount)
            LongestStreakCount = newStreakCount;

        SetUpdatedAt();
    }

    public void ResetStreak()
    {
        if (StreakCount > 0)
        {
            AddDomainEvent(new Events.StreakLostEvent(Id, StreakCount));
        }

        StreakCount = 0;
        LastStreakDate = null;
        SetUpdatedAt();
    }

    // ===== League =====

    public void ChangeTier(string newTier)
    {
        if (string.IsNullOrWhiteSpace(newTier))
            throw new DomainException("User.InvalidTier", "Tier cannot be empty.");

        CurrentTier = newTier;
        SetUpdatedAt();
    }

    // ===== Statistics =====

    public void IncrementTasksCompleted()
    {
        TotalTasksCompleted++;
        SetUpdatedAt();
    }

    public void IncrementTotalDuels()
    {
        TotalDuels++;
        SetUpdatedAt();
    }

    public void IncrementDuelsWon()
    {
        TotalDuelsWon++;
        SetUpdatedAt();
    }

    public void IncrementGlobalMissions()
    {
        TotalGlobalMissions++;
        SetUpdatedAt();
    }

    // ===== Social Counters (Updated via Friendship triggers) =====

    public void IncrementFollowingCount()
    {
        FollowingCount++;
        SetUpdatedAt();
    }

    public void DecrementFollowingCount()
    {
        FollowingCount = Math.Max(0, FollowingCount - 1);
        SetUpdatedAt();
    }

    public void IncrementFollowersCount()
    {
        FollowersCount++;
        SetUpdatedAt();
    }

    public void DecrementFollowersCount()
    {
        FollowersCount = Math.Max(0, FollowersCount - 1);
        SetUpdatedAt();
    }

    // ===== League Tier =====

    public void UpdateTier(string newTier)
    {
        if (string.IsNullOrWhiteSpace(newTier))
            throw new ArgumentException("Tier cannot be empty.", nameof(newTier));

        CurrentTier = newTier.ToUpperInvariant();
        SetUpdatedAt();
    }
}
