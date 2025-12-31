using HealthVerse.Contracts.Notifications;
using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Missions.Application.Commands;

public sealed record PokePartnerCommand(Guid MissionId, Guid UserId) : IRequest<PartnerMissionActionResponse>;

public sealed class PokePartnerCommandHandler : IRequestHandler<PokePartnerCommand, PartnerMissionActionResponse>
{
    private readonly IPartnerMissionRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IMissionsUserService _userService;
    private readonly IMissionsUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public PokePartnerCommandHandler(
        IPartnerMissionRepository repository,
        INotificationService notificationService,
        IMissionsUserService userService,
        IMissionsUnitOfWork unitOfWork,
        IClock clock)
    {
        _repository = repository;
        _notificationService = notificationService;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<PartnerMissionActionResponse> Handle(PokePartnerCommand request, CancellationToken ct)
    {
        var mission = await _repository.GetByIdAsync(request.MissionId, ct);
        if (mission is null)
        {
            return new PartnerMissionActionResponse { Success = false, Message = "Görev bulunamadı." };
        }

        if (!mission.IsParticipant(request.UserId))
        {
            return new PartnerMissionActionResponse { Success = false, Message = "Bu göreve dahil değilsiniz." };
        }

        var now = _clock.UtcNow;
        if (!mission.Poke(request.UserId, now))
        {
             return new PartnerMissionActionResponse
            {
                Success = false,
                Message = mission.Status != PartnerMissionStatus.ACTIVE
                    ? "Sadece aktif görevlerde partnerinizi dürtebilirsiniz."
                    : "Bugün zaten partnerinizi dürttünüz."
            };
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Notify Partner (PARTNER_POKE)
        var pokerUser = await _userService.GetUserAsync(request.UserId, ct);
        var pokerName = pokerUser?.Username ?? "Partnerin";
        var partnerId = mission.InitiatorId == request.UserId ? mission.PartnerId : mission.InitiatorId;

        await _notificationService.CreateAsync(
            partnerId,
            NotificationType.PARTNER_POKE,
            "Partnerin seni dürttü 🤝",
            $"{pokerName} ortak görev için bir hamle bekliyor!",
            mission.Id,
            "PARTNER_MISSION",
            null,
            ct);
        
        await _unitOfWork.SaveChangesAsync(ct);

        return new PartnerMissionActionResponse
        {
            Success = true,
            Message = "Partnerinize dürtme bildirimi gönderildi!"
        };
    }
}
