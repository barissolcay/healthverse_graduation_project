using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;

namespace HealthVerse.Competition.Domain.Entities;

/// <summary>
/// League member entity representing a user's membership in a room.
/// </summary>
public sealed class LeagueMember : Entity
{
    public Guid RoomId { get; private init; }
    public WeekId WeekId { get; private init; } = null!;
    public Guid UserId { get; private init; }
    public int PointsInRoom { get; private set; }
    public int? RankSnapshot { get; private set; }
    public DateTimeOffset JoinedAt { get; private init; }

    private LeagueMember() { }

    public static LeagueMember Create(Guid roomId, WeekId weekId, Guid userId)
    {
        return new LeagueMember
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            WeekId = weekId,
            UserId = userId,
            PointsInRoom = 0,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdatePoints(int newPoints)
    {
        if (newPoints < 0)
            throw new DomainException("LeagueMember.NegativePoints", "Points cannot be negative.");

        PointsInRoom = newPoints;
    }

    public void UpdateRank(int rank)
    {
        if (rank < 1)
            throw new DomainException("LeagueMember.InvalidRank", "Rank must be at least 1.");

        RankSnapshot = rank;
    }
}
