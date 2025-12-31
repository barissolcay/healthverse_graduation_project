using HealthVerse.Contracts.Gamification;
using HealthVerse.Gamification.Application.DTOs;
using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Gamification.Domain.Services;
using HealthVerse.Identity.Domain.Ports;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Gamification.Application.Commands;

// --- Command ---

public sealed record SyncStepsCommand(Guid UserId, int StepCount) : IRequest<StepSyncResponse>;

// --- Handler ---

public sealed class SyncStepsCommandHandler : IRequestHandler<SyncStepsCommand, StepSyncResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IPointTransactionRepository _transactionRepo;
    private readonly IUserDailyStatsRepository _dailyStatsRepo;
    private readonly IGamificationUnitOfWork _unitOfWork;
    private readonly PointCalculationService _pointCalculationService;
    private readonly IClock _clock;
    private readonly IMediator _mediator;
    private readonly ILogger<SyncStepsCommandHandler> _logger;

    public SyncStepsCommandHandler(
        IUserRepository userRepo,
        IPointTransactionRepository transactionRepo,
        IUserDailyStatsRepository dailyStatsRepo,
        IGamificationUnitOfWork unitOfWork,
        PointCalculationService pointCalculationService,
        IClock clock,
        IMediator mediator,
        ILogger<SyncStepsCommandHandler> logger)
    {
        _userRepo = userRepo;
        _transactionRepo = transactionRepo;
        _dailyStatsRepo = dailyStatsRepo;
        _unitOfWork = unitOfWork;
        _pointCalculationService = pointCalculationService;
        _clock = clock;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<StepSyncResponse> Handle(SyncStepsCommand request, CancellationToken ct)
    {
        if (request.StepCount <= 0)
        {
            return new StepSyncResponse
            {
                Success = false,
                Message = "Adım sayısı 0 veya negatif olamaz.",
                PointsEarned = 0
            };
        }

        // --- 1. GET USER ---
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            // For now, return generic error or handle auto-create if needed.
            // Assuming authorized user exists.
            return new StepSyncResponse { Success = false, Message = "Kullanıcı bulunamadı." };
        }

        // --- 2. IDEMPOTENCY CHECK ---
        var logDate = _clock.TodayTR;
        var key = IdempotencyKey.ForDailySteps(user.Id, logDate);

        var existingTransaction = await _transactionRepo.GetByIdempotencyKeyAsync(key.Value, ct);
        if (existingTransaction != null)
        {
            return new StepSyncResponse
            {
                Success = true,
                Message = "Bu veri zaten işlenmiş.",
                PointsEarned = existingTransaction.Amount,
                CurrentTotalPoints = user.TotalPoints,
                AlreadyProcessed = true
            };
        }

        // --- 3. UPDATE DAILY STATS (Overwrite) ---
        var dailyStats = await _dailyStatsRepo.GetByDateAsync(user.Id, logDate, ct);
        if (dailyStats == null)
        {
            dailyStats = UserDailyStats.Create(user.Id, logDate, request.StepCount);
            await _dailyStatsRepo.AddAsync(dailyStats, ct);
        }
        else
        {
            dailyStats.UpdateSteps(request.StepCount);
        }

        // --- 4. CALCULATE POINTS ---
        int pointsEarned = _pointCalculationService.CalculatePointsFromSteps(request.StepCount);

        if (pointsEarned > 0)
        {
            var transaction = PointTransaction.FromDailySteps(
                user.Id,
                pointsEarned,
                logDate,
                request.StepCount
            );

            await _transactionRepo.AddAsync(transaction, ct);
            
            // Domain logic on User entity (Identity context??)
            // Wait, User entity is in Identity Domain. Can we modify it here?
            // Ideally Gamification shouldn't modify Identity.User directly if contexts are strict.
            // But User entity here *has* TotalPoints.
            // Given "User" is the Identity user, and it has Points, it's a shared kernel aggregation or we are crossing boundaries.
            // The project structure puts User in Identity.Domain.Entities.
            // But it has Points logic.
            // We can update it because we have reference to the Entity.
            user.AddPoints(pointsEarned);

            // Update Daily Stats Points
            dailyStats.UpdatePoints(pointsEarned);

            // PUBLISH EVENT for other modules (Competition)
            await _mediator.Publish(new UserPointsEarnedEvent(user.Id, pointsEarned, "STEPS", logDate), ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Streak status
        bool streakSecured = request.StepCount >= 3000;

        return new StepSyncResponse
        {
            Success = true,
            Message = pointsEarned > 0
                ? $"Adımlar senkronize edildi! {request.StepCount} adım = {pointsEarned} puan."
                : streakSecured
                    ? $"Adımlar kaydedildi. Streak korundu! (3000+ adım)"
                    : $"Adımlar kaydedildi. Streak için {3000 - request.StepCount} adım daha gerekli.",
            PointsEarned = pointsEarned,
            CurrentTotalPoints = user.TotalPoints,
            AlreadyProcessed = false
        };
    }
}
