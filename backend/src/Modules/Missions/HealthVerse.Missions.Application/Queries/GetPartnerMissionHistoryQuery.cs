using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Missions.Application.Queries;

public sealed record GetPartnerMissionHistoryQuery(Guid UserId, int Limit = 10) : IRequest<PartnerMissionHistoryResponse>;

public sealed class GetPartnerMissionHistoryQueryHandler : IRequestHandler<GetPartnerMissionHistoryQuery, PartnerMissionHistoryResponse>
{
    private readonly IPartnerMissionRepository _repository;
    private readonly IMissionsUserService _userService;
    private readonly IClock _clock;

    public GetPartnerMissionHistoryQueryHandler(
        IPartnerMissionRepository repository,
        IMissionsUserService userService,
        IClock clock)
    {
        _repository = repository;
        _userService = userService;
        _clock = clock;
    }

    public async Task<PartnerMissionHistoryResponse> Handle(GetPartnerMissionHistoryQuery request, CancellationToken ct)
    {
        var limit = Math.Min(Math.Max(request.Limit, 1), 20);
        var history = await _repository.GetHistoryByUserAsync(request.UserId, limit, ct);

        if (!history.Any())
        {
             return new PartnerMissionHistoryResponse
            {
                Missions = new(),
                TotalCompleted = 0
            };
        }

        var userIds = history.SelectMany(m => new[] { m.InitiatorId, m.PartnerId }).Distinct();
        var users = await _userService.GetUsersAsync(userIds, ct);
        var now = _clock.UtcNow;

        var dtos = history.Select(m => MapToDetailDto(m, request.UserId, users, now)).ToList();

        return new PartnerMissionHistoryResponse
        {
            Missions = dtos,
            TotalCompleted = dtos.Count
        };
    }

    private static PartnerMissionDetailDto MapToDetailDto(
        WeeklyPartnerMission mission,
        Guid currentUserId,
        Dictionary<Guid, (string Username, int AvatarId)> users,
        DateTimeOffset now)
    {
        var (initiatorUsername, initiatorAvatarId) = users.GetValueOrDefault(mission.InitiatorId, ("Kullanıcı", 1));
        var (partnerUsername, partnerAvatarId) = users.GetValueOrDefault(mission.PartnerId, ("Kullanıcı", 1));

        var isInitiator = currentUserId == mission.InitiatorId;
        var canPoke = mission.Status == PartnerMissionStatus.ACTIVE;
        
        // History items usually are not active, but logic holds
        if (canPoke)
        {
            var lastPoke = isInitiator ? mission.InitiatorLastPokeAt : mission.PartnerLastPokeAt;
            if (lastPoke.HasValue && lastPoke.Value.Date == now.Date)
            {
                canPoke = false;
            }
        }

        return new PartnerMissionDetailDto
        {
            Id = mission.Id,
            WeekId = mission.WeekId,
            InitiatorId = mission.InitiatorId,
            InitiatorName = initiatorUsername,
            InitiatorAvatarId = initiatorAvatarId,
            InitiatorProgress = mission.InitiatorProgress,
            PartnerId = mission.PartnerId,
            PartnerName = partnerUsername,
            PartnerAvatarId = partnerAvatarId,
            PartnerProgress = mission.PartnerProgress,
            ActivityType = mission.ActivityType,
            TargetMetric = mission.TargetMetric,
            TargetValue = mission.TargetValue,
            TotalProgress = mission.TotalProgress,
            ProgressPercent = mission.ProgressPercent,
            Status = mission.Status,
            IsCompleted = mission.IsCompleted,
            IsInitiator = isInitiator,
            CanPoke = canPoke,
            CreatedAt = mission.CreatedAt
        };
    }
}
