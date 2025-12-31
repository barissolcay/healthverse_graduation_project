using HealthVerse.Contracts.Notifications;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Application.Commands;

public sealed record PokeDuelCommand(Guid DuelId, Guid UserId) : IRequest<DuelActionResponse>;

public sealed class PokeDuelCommandHandler : IRequestHandler<PokeDuelCommand, DuelActionResponse>
{
    private readonly IDuelRepository _duelRepo;
    private readonly INotificationService _notificationService;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<PokeDuelCommandHandler> _logger;

    public PokeDuelCommandHandler(
        IDuelRepository duelRepo,
        INotificationService notificationService,
        ISocialUnitOfWork unitOfWork,
        IClock clock,
        ILogger<PokeDuelCommandHandler> logger)
    {
        _duelRepo = duelRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<DuelActionResponse> Handle(PokeDuelCommand request, CancellationToken ct)
    {
        var duel = await _duelRepo.GetByIdAsync(request.DuelId, ct);
        if (duel == null)
        {
            return new DuelActionResponse { Success = false, Message = "Düello bulunamadı." };
        }

        if (!duel.IsParticipant(request.UserId))
        {
            return new DuelActionResponse { Success = false, Message = "Bu düelloda yetkiniz yok." };
        }

        var now = _clock.UtcNow;
        if (!duel.Poke(request.UserId, now))
        {
            return new DuelActionResponse { Success = false, Message = "Şu an dürtemezsiniz (günde 1 kez veya düello aktif değil)." };
        }

        try
        {
            // Notify Opponent (the other person)
            var targetId = duel.ChallengerId == request.UserId ? duel.OpponentId : duel.ChallengerId;

            await _notificationService.CreateAsync(
                targetId,
                NotificationType.DUEL_POKE,
                "Düello Hatırlatması! ⚡",
                "Rakibin seni dürttü! Hadi harekete geç!",
                duel.Id,
                "DUEL",
                null,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new DuelActionResponse { Success = true, Message = "Rakip dürtüldü!" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error poking duel {DuelId}", request.DuelId);
            return new DuelActionResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}
