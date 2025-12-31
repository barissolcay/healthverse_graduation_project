using HealthVerse.Contracts.Notifications;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Application.Commands;

public sealed record CreateDuelCommand(
    Guid ChallengerId,
    Guid OpponentId,
    string ActivityType,
    string TargetMetric,
    int TargetValue,
    int DurationDays) : IRequest<CreateDuelResponse>;

public sealed class CreateDuelCommandHandler : IRequestHandler<CreateDuelCommand, CreateDuelResponse>
{
    private readonly IDuelRepository _duelRepo;
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly INotificationService _notificationService;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDuelCommandHandler> _logger;

    public CreateDuelCommandHandler(
        IDuelRepository duelRepo,
        IFriendshipRepository friendshipRepo,
        INotificationService notificationService,
        ISocialUnitOfWork unitOfWork,
        ILogger<CreateDuelCommandHandler> logger)
    {
        _duelRepo = duelRepo;
        _friendshipRepo = friendshipRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateDuelResponse> Handle(CreateDuelCommand request, CancellationToken ct)
    {
        try
        {
            // 1. Check Friendship (Must be mutual friends)
            // Note: IFriendshipRepository.IsMutualAsync was defined in previous turns.
            bool isFriend = await _friendshipRepo.IsMutualAsync(request.ChallengerId, request.OpponentId, ct);
            if (!isFriend)
            {
                return new CreateDuelResponse { Success = false, Message = "Sadece arkadaşlarınızla düello yapabilirsiniz." };
            }

            // 2. Check Existing Duels
            bool hasActive = await _duelRepo.HasActiveOrPendingBetweenAsync(request.ChallengerId, request.OpponentId, ct);
            if (hasActive)
            {
                return new CreateDuelResponse { Success = false, Message = "Bu kişiyle zaten aktif veya bekleyen bir düellonuz var." };
            }

            // 3. Create Duel
            var duel = Duel.Create(
                request.ChallengerId,
                request.OpponentId,
                request.ActivityType,
                request.TargetMetric,
                request.TargetValue,
                request.DurationDays);

            await _duelRepo.AddAsync(duel, ct);

            // 4. Notify Opponent
            await _notificationService.CreateAsync(
                request.OpponentId,
                NotificationType.DUEL_REQUEST,
                "Yeni Düello İsteği! ⚔️",
                "Seni bir düelloya davet etti.",
                duel.Id,
                "DUEL",
                null,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new CreateDuelResponse
            {
                Success = true,
                Message = "Düello daveti gönderildi.",
                DuelId = duel.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating duel");
            return new CreateDuelResponse { Success = false, Message = ex.Message ?? "Bir hata oluştu." };
        }
    }
}
