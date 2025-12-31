using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Missions.Application.Queries;

public sealed record GetGlobalMissionDetailQuery(Guid MissionId, Guid UserId) : IRequest<GlobalMissionDetailDto?>;

public sealed class GetGlobalMissionDetailQueryHandler : IRequestHandler<GetGlobalMissionDetailQuery, GlobalMissionDetailDto?>
{
    private readonly IGlobalMissionRepository _missionRepository;
    private readonly IGlobalMissionParticipantRepository _participantRepository;
    private readonly IMissionsUserService _userService;
    private readonly IClock _clock;

    public GetGlobalMissionDetailQueryHandler(
        IGlobalMissionRepository missionRepository,
        IGlobalMissionParticipantRepository participantRepository,
        IMissionsUserService userService,
        IClock clock)
    {
        _missionRepository = missionRepository;
        _participantRepository = participantRepository;
        _userService = userService;
        _clock = clock;
    }

    public async Task<GlobalMissionDetailDto?> Handle(GetGlobalMissionDetailQuery request, CancellationToken ct)
    {
        var mission = await _missionRepository.GetByIdAsync(request.MissionId, ct);
        if (mission is null)
        {
            return null;
        }

        var now = _clock.UtcNow;

        // My participation
        var myParticipation = await _participantRepository.GetAsync(request.MissionId, request.UserId, ct);

        // Top contributors
        var topParticipants = await _participantRepository.GetTopContributorsAsync(mission.Id, 3, ct);
        var participantCount = await _participantRepository.CountAsync(mission.Id, ct);

        // User details
        var userIds = topParticipants.Select(p => p.UserId).Distinct();
        var users = await _userService.GetUsersAsync(userIds, ct);

        var topContributorDtos = topParticipants.Select((p, index) =>
        {
            var user = users.GetValueOrDefault(p.UserId, ("Kullanıcı", 1));
            return new TopContributorDto
            {
                UserId = p.UserId,
                Username = user.Item1,
                AvatarId = user.Item2,
                ContributionValue = p.ContributionValue,
                Rank = index + 1
            };
        }).ToList();

        return new GlobalMissionDetailDto
        {
            Id = mission.Id,
            Title = mission.Title,
            ActivityType = mission.ActivityType,
            TargetMetric = mission.TargetMetric,
            TargetValue = mission.TargetValue,
            CurrentValue = mission.CurrentValue,
            ProgressPercent = mission.ProgressPercent,
            Status = mission.Status,
            StartDate = mission.StartDate,
            EndDate = mission.EndDate,
            HoursRemaining = (int)Math.Max(0, (mission.EndDate - now).TotalHours),
            IsParticipant = myParticipation != null,
            MyContribution = myParticipation?.ContributionValue ?? 0,
            IsRewardClaimed = myParticipation?.IsRewardClaimed ?? false,
            TopContributors = topContributorDtos,
            TotalParticipants = participantCount
        };
    }
}
