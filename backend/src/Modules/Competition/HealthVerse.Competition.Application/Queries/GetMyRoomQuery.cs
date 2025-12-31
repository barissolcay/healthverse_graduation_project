using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Application.Ports;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;

namespace HealthVerse.Competition.Application.Queries;

public sealed record GetMyRoomQuery(Guid UserId) : IRequest<MyRoomResponse?>;

public sealed class GetMyRoomQueryHandler : IRequestHandler<GetMyRoomQuery, MyRoomResponse?>
{
    private readonly ILeagueMemberRepository _memberRepository;
    private readonly ILeagueRoomRepository _roomRepository;
    private readonly ILeagueConfigRepository _configRepository;
    private readonly IClock _clock;

    public GetMyRoomQueryHandler(
        ILeagueMemberRepository memberRepository,
        ILeagueRoomRepository roomRepository,
        ILeagueConfigRepository configRepository,
        IClock clock)
    {
        _memberRepository = memberRepository;
        _roomRepository = roomRepository;
        _configRepository = configRepository;
        _clock = clock;
    }

    public async Task<MyRoomResponse?> Handle(GetMyRoomQuery request, CancellationToken ct)
    {
        var currentWeekId = WeekId.FromDate(_clock.TodayTR);

        var membership = await _memberRepository.GetMembershipByUserAndWeekAsync(request.UserId, currentWeekId, ct);
        if (membership is null)
            return null;

        var room = await _roomRepository.GetByIdAsync(membership.RoomId, ct);
        if (room is null)
            return null;

        var tierConfig = await _configRepository.GetByTierNameAsync(room.Tier, ct);
        var rankInRoom = await _memberRepository.GetRankForUserAsync(room.Id, membership.PointsInRoom, ct);
        var totalMembers = await _memberRepository.CountByRoomAsync(room.Id, ct);
        var hoursRemaining = (int)Math.Max(0, (room.EndsAt - _clock.UtcNow).TotalHours);

        return new MyRoomResponse
        {
            RoomId = room.Id,
            WeekId = room.WeekId.Value,
            Tier = room.Tier,
            TierOrder = tierConfig?.TierOrder ?? 1,
            RankInRoom = rankInRoom,
            PointsInRoom = membership.PointsInRoom,
            TotalMembers = totalMembers,
            StartsAt = room.StartsAt,
            EndsAt = room.EndsAt,
            IsProcessed = room.IsProcessed,
            HoursRemaining = hoursRemaining
        };
    }
}
