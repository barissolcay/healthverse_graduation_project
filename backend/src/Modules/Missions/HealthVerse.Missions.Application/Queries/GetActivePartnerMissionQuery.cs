using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;

namespace HealthVerse.Missions.Application.Queries;

public sealed record GetActivePartnerMissionQuery(Guid UserId) : IRequest<PartnerMissionDetailDto?>;

public sealed class GetActivePartnerMissionQueryHandler : IRequestHandler<GetActivePartnerMissionQuery, PartnerMissionDetailDto?>
{
    private readonly IPartnerMissionRepository _repository;
    private readonly IMissionsUserService _userService;
    private readonly IClock _clock;

    public GetActivePartnerMissionQueryHandler(
        IPartnerMissionRepository repository,
        IMissionsUserService userService,
        IClock clock)
    {
        _repository = repository;
        _userService = userService;
        _clock = clock;
    }

    public async Task<PartnerMissionDetailDto?> Handle(GetActivePartnerMissionQuery request, CancellationToken ct)
    {
        var currentWeekId = WeekId.FromDate(_clock.TodayTR).Value;
        
        var mission = await _repository.GetActiveByUserAsync(currentWeekId, request.UserId, ct);
        if (mission is null)
        {
            return null;
        }

        var users = await _userService.GetUsersAsync(new[] { mission.InitiatorId, mission.PartnerId }, ct);
        var now = _clock.UtcNow;

        return MapToDetailDto(mission, request.UserId, users, now);
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
