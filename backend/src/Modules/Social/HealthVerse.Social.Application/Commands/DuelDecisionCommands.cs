using HealthVerse.Contracts.Notifications;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Application.Commands;

// --- Accept Command ---
public sealed record AcceptDuelCommand(Guid DuelId, Guid UserId) : IRequest<DuelActionResponse>;

public sealed class AcceptDuelCommandHandler : IRequestHandler<AcceptDuelCommand, DuelActionResponse>
{
    private readonly IDuelRepository _duelRepo;
    private readonly INotificationService _notificationService;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<AcceptDuelCommandHandler> _logger;

    public AcceptDuelCommandHandler(
        IDuelRepository duelRepo,
        INotificationService notificationService,
        ISocialUnitOfWork unitOfWork,
        IClock clock,
        ILogger<AcceptDuelCommandHandler> logger)
    {
        _duelRepo = duelRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<DuelActionResponse> Handle(AcceptDuelCommand request, CancellationToken ct)
    {
        var duel = await _duelRepo.GetByIdAsync(request.DuelId, ct);
        if (duel == null)
        {
            return new DuelActionResponse { Success = false, Message = "Düello bulunamadı." };
        }

        // Only opponent can accept
        if (duel.OpponentId != request.UserId)
        {
            return new DuelActionResponse { Success = false, Message = "Bu düelloyu kabul etme yetkiniz yok." };
        }

        var now = _clock.UtcNow;
        if (!duel.Accept(now))
        {
            return new DuelActionResponse { Success = false, Message = "Düello kabul edilebilir durumda değil (süresi dolmuş veya zaten yanıtlanmış olabilir)." };
        }

        try
        {
            // Notify Challenger
            await _notificationService.CreateAsync(
                duel.ChallengerId,
                NotificationType.DUEL_ACCEPTED,
                "Düello Kabul Edildi! ⚔️",
                "Rakibin meydan okumanı kabul etti. Mücadele başlasın!",
                duel.Id,
                "DUEL",
                null,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new DuelActionResponse { Success = true, Message = "Düello kabul edildi." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting duel {DuelId}", request.DuelId);
            return new DuelActionResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}

// --- Reject Command ---
public sealed record RejectDuelCommand(Guid DuelId, Guid UserId) : IRequest<DuelActionResponse>;

public sealed class RejectDuelCommandHandler : IRequestHandler<RejectDuelCommand, DuelActionResponse>
{
    private readonly IDuelRepository _duelRepo;
    private readonly INotificationService _notificationService;
    private readonly ISocialUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<RejectDuelCommandHandler> _logger;

    public RejectDuelCommandHandler(
        IDuelRepository duelRepo,
        INotificationService notificationService,
        ISocialUnitOfWork unitOfWork,
        IClock clock,
        ILogger<RejectDuelCommandHandler> logger)
    {
        _duelRepo = duelRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<DuelActionResponse> Handle(RejectDuelCommand request, CancellationToken ct)
    {
        var duel = await _duelRepo.GetByIdAsync(request.DuelId, ct);
        if (duel == null)
        {
            return new DuelActionResponse { Success = false, Message = "Düello bulunamadı." };
        }

        if (duel.OpponentId != request.UserId)
        {
            return new DuelActionResponse { Success = false, Message = "Bu düelloyu reddetme yetkiniz yok." };
        }

        var now = _clock.UtcNow;
        if (!duel.Reject(now))
        {
            return new DuelActionResponse { Success = false, Message = "Düello reddedilebilir durumda değil." };
        }

        try
        {
            // Notify Challenger
            await _notificationService.CreateAsync(
                duel.ChallengerId,
                NotificationType.DUEL_REJECTED,
                "Düello Reddedildi ❌",
                "Rakibin meydan okumanı reddetti.",
                duel.Id,
                "DUEL",
                null,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new DuelActionResponse { Success = true, Message = "Düello reddedildi." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting duel {DuelId}", request.DuelId);
            return new DuelActionResponse { Success = false, Message = "Bir hata oluştu." };
        }
    }
}
