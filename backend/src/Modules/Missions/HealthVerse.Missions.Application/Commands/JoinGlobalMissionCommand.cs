using HealthVerse.Contracts.Notifications;
using HealthVerse.Missions.Application.DTOs;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Missions.Application.Commands;

public sealed record JoinGlobalMissionCommand(Guid MissionId, Guid UserId) : IRequest<JoinMissionResponse>;

public sealed class JoinGlobalMissionCommandHandler : IRequestHandler<JoinGlobalMissionCommand, JoinMissionResponse>
{
    private readonly IGlobalMissionRepository _missionRepository;
    private readonly IGlobalMissionParticipantRepository _participantRepository;
    private readonly INotificationService _notificationService;
    private readonly IMissionsUnitOfWork _unitOfWork;
    private readonly ILogger<JoinGlobalMissionCommandHandler> _logger;

    public JoinGlobalMissionCommandHandler(
        IGlobalMissionRepository missionRepository,
        IGlobalMissionParticipantRepository participantRepository,
        INotificationService notificationService,
        IMissionsUnitOfWork unitOfWork,
        ILogger<JoinGlobalMissionCommandHandler> logger)
    {
        _missionRepository = missionRepository;
        _participantRepository = participantRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<JoinMissionResponse> Handle(JoinGlobalMissionCommand request, CancellationToken ct)
    {
        var mission = await _missionRepository.GetByIdAsync(request.MissionId, ct);
        if (mission is null)
        {
            return new JoinMissionResponse { Success = false, Message = "Görev bulunamadı." };
        }

        if (!mission.IsActive)
        {
            return new JoinMissionResponse { Success = false, Message = "Bu görev aktif değil." };
        }

        // Check already joined
        var existing = await _participantRepository.GetAsync(request.MissionId, request.UserId, ct);
        if (existing != null)
        {
            return new JoinMissionResponse { Success = true, Message = "Zaten bu göreve katılmışsınız." };
        }

        try
        {
            var participant = GlobalMissionParticipant.Create(request.MissionId, request.UserId);
            await _participantRepository.AddAsync(participant, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Notify
            await _notificationService.CreateAsync(
                request.UserId,
                NotificationType.GLOBAL_MISSION_JOINED,
                "Göreve katıldın! 🌍",
                $"\"{mission.Title}\" görevine katıldın. İlk katkını yapınca ödül şansın başlar!",
                mission.Id,
                "GLOBAL_MISSION",
                null,
                ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new JoinMissionResponse
            {
                Success = true,
                Message = "Göreve katıldınız!"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error participating in global mission");
            return new JoinMissionResponse { Success = false, Message = "Katılım sırasında bir hata oluştu." };
        }
    }
}
