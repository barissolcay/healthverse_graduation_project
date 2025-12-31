using HealthVerse.Contracts.Notifications;
using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Missions.Application.Commands;

public sealed record PairWithFriendCommand(Guid UserId, Guid FriendId) : IRequest<PairPartnerResponse>;

public sealed class PairWithFriendCommandHandler : IRequestHandler<PairWithFriendCommand, PairPartnerResponse>
{
    private readonly IPartnerMissionRepository _missionRepository;
    private readonly IPartnerMissionSlotRepository _slotRepository;
    private readonly IFriendshipService _friendshipService;
    private readonly INotificationService _notificationService;
    private readonly IMissionsUserService _userService;
    private readonly IMissionsUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<PairWithFriendCommandHandler> _logger;

    public PairWithFriendCommandHandler(
        IPartnerMissionRepository missionRepository,
        IPartnerMissionSlotRepository slotRepository,
        IFriendshipService friendshipService,
        INotificationService notificationService,
        IMissionsUserService userService,
        IMissionsUnitOfWork unitOfWork,
        IClock clock,
        ILogger<PairWithFriendCommandHandler> logger)
    {
        _missionRepository = missionRepository;
        _slotRepository = slotRepository;
        _friendshipService = friendshipService;
        _notificationService = notificationService;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PairPartnerResponse> Handle(PairWithFriendCommand request, CancellationToken ct)
    {
        if (request.UserId == request.FriendId)
        {
            return new PairPartnerResponse { Success = false, Message = "Kendinizle eşleşemezsiniz." };
        }

        var currentWeekId = WeekId.FromDate(_clock.TodayTR).Value;

        // 1. Check mutual friendship
        var isMutual = await _friendshipService.IsMutualFriendAsync(request.UserId, request.FriendId, ct);
        if (!isMutual)
        {
            return new PairPartnerResponse { Success = false, Message = "Sadece karşılıklı takip ettiğiniz arkadaşlarınızla eşleşebilirsiniz." };
        }

        // 2. Check current user busy
        if (await _slotRepository.IsUserBusyAsync(currentWeekId, request.UserId, ct))
        {
            return new PairPartnerResponse { Success = false, Message = "Bu hafta zaten bir partner göreviniz var." };
        }

        // 3. Check friend busy
        if (await _slotRepository.IsUserBusyAsync(currentWeekId, request.FriendId, ct))
        {
            return new PairPartnerResponse { Success = false, Message = "Bu arkadaşınızın zaten bir partner görevi var." };
        }

        try
        {
            // 4. Create Mission
            var mission = WeeklyPartnerMission.Create(currentWeekId, request.UserId, request.FriendId);
            await _missionRepository.AddAsync(mission, ct);

            // 5. Create Slots
            var slot1 = WeeklyPartnerMissionSlot.Create(currentWeekId, request.UserId, mission.Id);
            var slot2 = WeeklyPartnerMissionSlot.Create(currentWeekId, request.FriendId, mission.Id); // Uses FriendId
            
            await _slotRepository.AddAsync(slot1, ct);
            await _slotRepository.AddAsync(slot2, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            // 6. Notify Partner (NotificationType.PARTNER_MATCHED)
            var initiatorInfo = await _userService.GetUserAsync(request.UserId, ct);
            var initiatorName = initiatorInfo?.Username ?? "Birisi";
            
            await _notificationService.CreateAsync(
                request.FriendId,
                NotificationType.PARTNER_MATCHED,
                "Yeni ortak görev! 🤝",
                $"Bu hafta {initiatorName} ile ortak görev başladı!",
                mission.Id,
                "PARTNER_MISSION",
                null,
                ct);
            
            await _unitOfWork.SaveChangesAsync(ct);

            return new PairPartnerResponse
            {
                Success = true,
                Message = "Partner görevi başladı!",
                MissionId = mission.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating partner mission");
            return new PairPartnerResponse { Success = false, Message = "Eşleşme sırasında bir hata oluştu." };
        }
    }
}
