using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Missions.Application.Queries;

public sealed record GetGlobalMissionHistoryQuery(Guid UserId, int Limit = 10) : IRequest<ActiveGlobalMissionsResponse>;

public sealed class GetGlobalMissionHistoryQueryHandler : IRequestHandler<GetGlobalMissionHistoryQuery, ActiveGlobalMissionsResponse>
{
    private readonly IGlobalMissionRepository _missionRepository;
    private readonly IGlobalMissionParticipantRepository _participantRepository;
    private readonly IClock _clock;

    public GetGlobalMissionHistoryQueryHandler(
        IGlobalMissionRepository missionRepository,
        IGlobalMissionParticipantRepository participantRepository,
        IClock clock)
    {
        _missionRepository = missionRepository;
        _participantRepository = participantRepository;
        _clock = clock;
    }

    public async Task<ActiveGlobalMissionsResponse> Handle(GetGlobalMissionHistoryQuery request, CancellationToken ct)
    {
        var limit = Math.Min(Math.Max(request.Limit, 1), 20);
        var now = _clock.UtcNow;

        var historyMissions = await _missionRepository.GetHistoryAsync(now, limit, ct);

        if (!historyMissions.Any())
        {
             return new ActiveGlobalMissionsResponse
            {
                Missions = new(),
                TotalActive = 0
            };
        }

        var missionIds = historyMissions.Select(m => m.Id).ToList();

        // My participations
        var myParticipations = await _participantRepository.GetByUserAsync(request.UserId, missionIds, ct);

        var dtos = historyMissions.Select(m =>
        {
            myParticipations.TryGetValue(m.Id, out var myParticipation);
            return new GlobalMissionDetailDto
            {
                Id = m.Id,
                Title = m.Title,
                ActivityType = m.ActivityType,
                TargetMetric = m.TargetMetric,
                TargetValue = m.TargetValue,
                CurrentValue = m.CurrentValue,
                ProgressPercent = m.ProgressPercent,
                Status = m.Status,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                HoursRemaining = 0,
                IsParticipant = myParticipation != null,
                MyContribution = myParticipation?.ContributionValue ?? 0,
                IsRewardClaimed = myParticipation?.IsRewardClaimed ?? false,
                TopContributors = new List<TopContributorDto>(), // usually empty for list view
                TotalParticipants = 0 // Optimization: skip count for history list
            };
        }).ToList();

        return new ActiveGlobalMissionsResponse
        {
            Missions = dtos,
            TotalActive = dtos.Count // Reusing TotalActive as count field
        };
    }
}
