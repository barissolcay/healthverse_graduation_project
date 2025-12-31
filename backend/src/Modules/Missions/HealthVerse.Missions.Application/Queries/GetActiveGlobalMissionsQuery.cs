using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Missions.Application.Queries;

public sealed record GetActiveGlobalMissionsQuery(Guid UserId) : IRequest<ActiveGlobalMissionsResponse>;

public sealed class GetActiveGlobalMissionsQueryHandler : IRequestHandler<GetActiveGlobalMissionsQuery, ActiveGlobalMissionsResponse>
{
    private readonly IGlobalMissionRepository _missionRepository;
    private readonly IGlobalMissionParticipantRepository _participantRepository;
    private readonly IMissionsUserService _userService;
    private readonly IClock _clock;

    public GetActiveGlobalMissionsQueryHandler(
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

    public async Task<ActiveGlobalMissionsResponse> Handle(GetActiveGlobalMissionsQuery request, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var activeMissions = await _missionRepository.GetActiveAsync(now, ct);

        if (!activeMissions.Any())
        {
            return new ActiveGlobalMissionsResponse { Missions = new(), TotalActive = 0 };
        }

        var missionIds = activeMissions.Select(m => m.Id).ToList();

        // My participations
        var myParticipations = await _participantRepository.GetByUserAsync(request.UserId, missionIds, ct);

        var dtos = new List<GlobalMissionDetailDto>();

        foreach (var mission in activeMissions)
        {
            // Top contributors
            var topParticipants = await _participantRepository.GetTopContributorsAsync(mission.Id, 3, ct);
            var participantCount = await _participantRepository.CountAsync(mission.Id, ct);

            // User details for top contributors
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

            myParticipations.TryGetValue(mission.Id, out var myParticipation);

            dtos.Add(new GlobalMissionDetailDto
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
            });
        }

        return new ActiveGlobalMissionsResponse
        {
            Missions = dtos,
            TotalActive = dtos.Count
        };
    }
}
