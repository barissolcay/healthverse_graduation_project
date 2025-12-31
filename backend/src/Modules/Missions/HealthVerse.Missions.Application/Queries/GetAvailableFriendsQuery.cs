using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;

namespace HealthVerse.Missions.Application.Queries;

public sealed record GetAvailableFriendsQuery(Guid UserId) : IRequest<AvailableFriendsResponse>;

public sealed class GetAvailableFriendsQueryHandler : IRequestHandler<GetAvailableFriendsQuery, AvailableFriendsResponse>
{
    private readonly IFriendshipService _friendshipService;
    private readonly IPartnerMissionSlotRepository _slotRepository;
    private readonly IMissionsUserService _userService;
    private readonly IClock _clock;

    public GetAvailableFriendsQueryHandler(
        IFriendshipService friendshipService,
        IPartnerMissionSlotRepository slotRepository,
        IMissionsUserService userService,
        IClock clock)
    {
        _friendshipService = friendshipService;
        _slotRepository = slotRepository;
        _userService = userService;
        _clock = clock;
    }

    public async Task<AvailableFriendsResponse> Handle(GetAvailableFriendsQuery request, CancellationToken ct)
    {
        var currentWeekId = WeekId.FromDate(_clock.TodayTR).Value;

        // 1. Check if I am busy
        var isSelfBusy = await _slotRepository.IsUserBusyAsync(currentWeekId, request.UserId, ct);
        if (isSelfBusy)
        {
            return new AvailableFriendsResponse
            {
                Friends = new List<AvailableFriendDto>(),
                TotalAvailable = 0
            };
        }

        // 2. Get mutual friends
        var mutualFriendIds = await _friendshipService.GetMutualFriendIdsAsync(request.UserId, ct);
        if (!mutualFriendIds.Any())
        {
            return new AvailableFriendsResponse { Friends = new(), TotalAvailable = 0 };
        }

        // 3. Filter busy friends
        // Note: Doing this in loop might be N+1 queries if IsUserBusyAsync is single check.
        // Ideally we should have GetBusyUsersAsync(userIds).
        // For MVP, N+1 on small friend list is okay, or we can improve Port later.
        // Assuming friend list < 100.
        
        var availableIds = new List<Guid>();
        foreach (var fid in mutualFriendIds)
        {
            if (!await _slotRepository.IsUserBusyAsync(currentWeekId, fid, ct))
            {
                availableIds.Add(fid);
            }
        }

        if (!availableIds.Any())
        {
            return new AvailableFriendsResponse { Friends = new(), TotalAvailable = 0 };
        }

        // 4. Get User details
        var users = await _userService.GetUsersAsync(availableIds, ct);

        var friendDtos = users.Select(u => new AvailableFriendDto
        {
            UserId = u.Key,
            Username = u.Value.Username,
            AvatarId = u.Value.AvatarId
        }).ToList();

        return new AvailableFriendsResponse
        {
            Friends = friendDtos,
            TotalAvailable = friendDtos.Count
        };
    }
}
