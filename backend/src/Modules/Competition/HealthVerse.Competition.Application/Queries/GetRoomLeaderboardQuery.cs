using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Application.Ports;
using MediatR;

namespace HealthVerse.Competition.Application.Queries;

public sealed record GetRoomLeaderboardQuery(Guid RoomId, Guid CurrentUserId) : IRequest<RoomLeaderboardResponse?>;

public sealed class GetRoomLeaderboardQueryHandler : IRequestHandler<GetRoomLeaderboardQuery, RoomLeaderboardResponse?>
{
    private readonly ILeagueRoomRepository _roomRepository;
    private readonly ILeagueMemberRepository _memberRepository;
    private readonly ILeagueConfigRepository _configRepository;
    private readonly ICompetitionUserRepository _userRepository;

    public GetRoomLeaderboardQueryHandler(
        ILeagueRoomRepository roomRepository,
        ILeagueMemberRepository memberRepository,
        ILeagueConfigRepository configRepository,
        ICompetitionUserRepository userRepository)
    {
        _roomRepository = roomRepository;
        _memberRepository = memberRepository;
        _configRepository = configRepository;
        _userRepository = userRepository;
    }

    public async Task<RoomLeaderboardResponse?> Handle(GetRoomLeaderboardQuery request, CancellationToken ct)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, ct);
        if (room is null)
            return null;

        var tierConfig = await _configRepository.GetByTierNameAsync(room.Tier, ct);
        var members = await _memberRepository.GetMembersByRoomOrderedAsync(request.RoomId, ct);
        var userIds = members.Select(m => m.UserId);
        var users = await _userRepository.GetByIdsAsync(userIds, ct);

        var totalMembers = members.Count;
        var promotionCutoff = tierConfig is not null
            ? (int)Math.Ceiling(totalMembers * tierConfig.PromotePercentage / 100.0)
            : 0;
        var demotionCutoff = tierConfig is not null
            ? totalMembers - (int)Math.Floor(totalMembers * tierConfig.DemotePercentage / 100.0) + 1
            : totalMembers + 1;

        var rank = 0;
        var memberDtos = members.Select(m =>
        {
            rank++;
            var user = users.GetValueOrDefault(m.UserId);
            return new RoomMemberDto
            {
                Rank = rank,
                UserId = m.UserId,
                Username = user?.Username.Value ?? "Unknown",
                AvatarId = user?.AvatarId ?? 1,
                PointsInRoom = m.PointsInRoom,
                InPromotionZone = rank <= promotionCutoff && tierConfig?.PromotePercentage > 0,
                InDemotionZone = rank >= demotionCutoff && tierConfig?.DemotePercentage > 0,
                IsCurrentUser = m.UserId == request.CurrentUserId
            };
        }).ToList();

        return new RoomLeaderboardResponse
        {
            RoomId = room.Id,
            WeekId = room.WeekId.Value,
            Tier = room.Tier,
            TotalMembers = totalMembers,
            PromotionCutoff = promotionCutoff,
            DemotionCutoff = demotionCutoff,
            Members = memberDtos
        };
    }
}
