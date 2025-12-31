using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;
using HealthVerse.SharedKernel.Results;

namespace HealthVerse.Competition.Domain.Entities;

/// <summary>
/// League room entity representing a weekly competition room.
/// Contains capacity validation as a domain rule.
/// </summary>
public sealed class LeagueRoom : AggregateRoot
{
    public WeekId WeekId { get; private init; } = null!;
    public string Tier { get; private init; } = null!;
    public int UserCount { get; private set; }
    public DateTimeOffset StartsAt { get; private init; }
    public DateTimeOffset EndsAt { get; private init; }
    public bool IsProcessed { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private readonly List<LeagueMember> _members = new();
    public IReadOnlyList<LeagueMember> Members => _members.AsReadOnly();

    private LeagueRoom() { }

    public static LeagueRoom Create(
        WeekId weekId,
        string tier,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (string.IsNullOrWhiteSpace(tier))
            throw new DomainException("LeagueRoom.InvalidTier", "Tier cannot be empty.");

        if (endsAt <= startsAt)
            throw new DomainException("LeagueRoom.InvalidTimeWindow", "EndsAt must be after StartsAt.");

        return new LeagueRoom
        {
            Id = Guid.NewGuid(),
            WeekId = weekId,
            Tier = tier,
            StartsAt = startsAt,
            EndsAt = endsAt,
            UserCount = 0,
            IsProcessed = false
        };
    }

    /// <summary>
    /// Adds a member to the room with capacity validation.
    /// </summary>
    public Result AddMember(Guid userId, int maxRoomSize)
    {
        // Domain rule: capacity check
        if (UserCount >= maxRoomSize)
            return Result.Failure(new Error("LeagueRoom.Full", "League room is full."));

        // Domain rule: no duplicate members
        if (_members.Any(m => m.UserId == userId))
            return Result.Failure(new Error("LeagueRoom.AlreadyMember", "User is already a member."));

        var member = LeagueMember.Create(Id, WeekId, userId);
        _members.Add(member);
        UserCount++;
        SetUpdatedAt();

        return Result.Success();
    }

    /// <summary>
    /// Removes a member from the room.
    /// </summary>
    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            _members.Remove(member);
            UserCount = Math.Max(0, UserCount - 1);
            SetUpdatedAt();
        }
    }

    /// <summary>
    /// Marks the room as processed (week finalized).
    /// Uses optimistic concurrency to prevent race conditions.
    /// </summary>
    public bool TryMarkAsProcessed(DateTimeOffset processedAt)
    {
        if (IsProcessed)
            return false;

        IsProcessed = true;
        ProcessedAt = processedAt;
        SetUpdatedAt();
        return true;
    }

    /// <summary>
    /// Increments the user count for the room.
    /// Used when adding members through repository operations.
    /// </summary>
    public void IncrementUserCount()
    {
        UserCount++;
        SetUpdatedAt();
    }
}
