using HealthVerse.Contracts.Health;
using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Gamification.Domain.Services;
using HealthVerse.Identity.Domain.Ports;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Gamification.Application.Commands;

// --- Command ---

/// <summary>
/// Command for synchronizing health data from Flutter Health package.
/// This is the main entry point for all health data synchronization.
/// </summary>
public sealed record SyncHealthDataCommand(
    Guid UserId,
    List<HealthActivityData> Activities) : IRequest<HealthSyncResponse>;

// --- Handler ---

/// <summary>
/// Handles health data synchronization.
/// Coordinates updates across all modules: Steps/Points, Goals, Tasks, Duels, Missions.
/// </summary>
public sealed class SyncHealthDataCommandHandler : IRequestHandler<SyncHealthDataCommand, HealthSyncResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IPointTransactionRepository _transactionRepo;
    private readonly IUserDailyStatsRepository _dailyStatsRepo;
    private readonly IGamificationUnitOfWork _unitOfWork;
    private readonly PointCalculationService _pointCalculationService;
    private readonly IClock _clock;
    private readonly IMediator _mediator;
    private readonly IEnumerable<IHealthProgressUpdater> _progressUpdaters;
    private readonly ILogger<SyncHealthDataCommandHandler> _logger;

    public SyncHealthDataCommandHandler(
        IUserRepository userRepo,
        IPointTransactionRepository transactionRepo,
        IUserDailyStatsRepository dailyStatsRepo,
        IGamificationUnitOfWork unitOfWork,
        PointCalculationService pointCalculationService,
        IClock clock,
        IMediator mediator,
        IEnumerable<IHealthProgressUpdater> progressUpdaters,
        ILogger<SyncHealthDataCommandHandler> logger)
    {
        _userRepo = userRepo;
        _transactionRepo = transactionRepo;
        _dailyStatsRepo = dailyStatsRepo;
        _unitOfWork = unitOfWork;
        _pointCalculationService = pointCalculationService;
        _clock = clock;
        _mediator = mediator;
        _progressUpdaters = progressUpdaters;
        _logger = logger;
    }

    public async Task<HealthSyncResponse> Handle(SyncHealthDataCommand request, CancellationToken ct)
    {
        // --- 1. VALIDATION ---
        if (request.Activities == null || request.Activities.Count == 0)
        {
            return new HealthSyncResponse
            {
                Success = false,
                Message = "En az bir aktivite verisi gerekli."
            };
        }

        // Filter out invalid recording methods (MANUAL, UNKNOWN)
        var validActivities = request.Activities
            .Where(a => a.RecordingMethod is RecordingMethod.AUTOMATIC or RecordingMethod.ACTIVE)
            .ToList();

        int rejectedCount = request.Activities.Count - validActivities.Count;
        
        if (validActivities.Count == 0)
        {
            return new HealthSyncResponse
            {
                Success = false,
                Message = "Tüm aktiviteler reddedildi (manuel veya bilinmeyen kayıt yöntemi).",
                RejectedActivities = rejectedCount
            };
        }

        // Validate metrics
        foreach (var activity in validActivities)
        {
            if (!TargetMetrics.IsValid(activity.TargetMetric))
            {
                return new HealthSyncResponse
                {
                    Success = false,
                    Message = $"Geçersiz metrik: {activity.TargetMetric}"
                };
            }

            if (activity.Value < 0)
            {
                return new HealthSyncResponse
                {
                    Success = false,
                    Message = "Aktivite değeri negatif olamaz."
                };
            }
        }

        // --- 2. GET USER ---
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            return new HealthSyncResponse
            {
                Success = false,
                Message = "Kullanıcı bulunamadı."
            };
        }

        var logDate = _clock.TodayTR;

        // --- 3. EXTRACT STEPS FROM ACTIVITIES ---
        // Get the highest STEPS value (Flutter may send multiple step entries)
        int totalSteps = validActivities
            .Where(a => a.TargetMetric.Equals(TargetMetrics.STEPS, StringComparison.OrdinalIgnoreCase))
            .Select(a => a.Value)
            .DefaultIfEmpty(0)
            .Max();

        // --- 4. IDEMPOTENCY CHECK FOR DAILY STEPS ---
        var stepIdempotencyKey = IdempotencyKey.ForDailySteps(user.Id, logDate);
        var existingStepTransaction = await _transactionRepo.GetByIdempotencyKeyAsync(stepIdempotencyKey.Value, ct);
        bool stepsAlreadyProcessed = existingStepTransaction != null;

        int stepPointsEarned = 0;

        if (!stepsAlreadyProcessed && totalSteps > 0)
        {
            // --- 5. UPDATE DAILY STATS ---
            var dailyStats = await _dailyStatsRepo.GetByDateAsync(user.Id, logDate, ct);
            if (dailyStats == null)
            {
                dailyStats = UserDailyStats.Create(user.Id, logDate, totalSteps);
                await _dailyStatsRepo.AddAsync(dailyStats, ct);
            }
            else
            {
                dailyStats.UpdateSteps(totalSteps);
            }

            // --- 6. CALCULATE AND RECORD STEP POINTS ---
            stepPointsEarned = _pointCalculationService.CalculatePointsFromSteps(totalSteps);

            if (stepPointsEarned > 0)
            {
                var transaction = PointTransaction.FromDailySteps(
                    user.Id,
                    stepPointsEarned,
                    logDate,
                    totalSteps
                );

                await _transactionRepo.AddAsync(transaction, ct);
                user.AddPoints(stepPointsEarned);
                dailyStats.UpdatePoints(stepPointsEarned);
            }
        }

        // --- 7. RUN ALL MODULE PROGRESS UPDATERS ---
        var orderedUpdaters = _progressUpdaters.OrderBy(u => u.Order);
        var updateResults = new List<HealthProgressResult>();
        int taskPointsEarned = 0;

        foreach (var updater in orderedUpdaters)
        {
            try
            {
                var result = await updater.UpdateProgressAsync(
                    request.UserId,
                    validActivities,
                    logDate,
                    ct);

                updateResults.Add(result);
                taskPointsEarned += result.PointsEarned;

                _logger.LogDebug(
                    "Module {Module} updated: {Updated} entities, {Completed} completed, {Points} points",
                    result.ModuleName, result.UpdatedCount, result.CompletedCount, result.PointsEarned);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Module} progress updater", updater.GetType().Name);
                // Continue with other updaters even if one fails
            }
        }

        // --- 8. ADD TASK POINTS TO USER ---
        if (taskPointsEarned > 0)
        {
            user.AddPoints(taskPointsEarned);
        }

        // --- 9. SAVE ALL CHANGES ---
        await _unitOfWork.SaveChangesAsync(ct);

        // --- 10. PUBLISH EVENT ---
        await _mediator.Publish(new HealthDataSyncedEvent(
            request.UserId,
            validActivities,
            logDate,
            totalSteps,
            stepPointsEarned + taskPointsEarned), ct);

        // --- 11. BUILD RESPONSE ---
        bool streakSecured = totalSteps >= 3000;

        // Aggregate results from all updaters
        var goalsResult = updateResults.FirstOrDefault(r => r.ModuleName == "Goals");
        var tasksResult = updateResults.FirstOrDefault(r => r.ModuleName == "Tasks");
        var duelsResult = updateResults.FirstOrDefault(r => r.ModuleName == "Duels");
        var partnerResult = updateResults.FirstOrDefault(r => r.ModuleName == "PartnerMissions");
        var globalResult = updateResults.FirstOrDefault(r => r.ModuleName == "GlobalMissions");

        string message = BuildResponseMessage(
            totalSteps, stepPointsEarned, taskPointsEarned, streakSecured,
            goalsResult?.CompletedCount ?? 0,
            tasksResult?.CompletedCount ?? 0,
            duelsResult?.CompletedCount ?? 0);

        return new HealthSyncResponse
        {
            Success = true,
            Message = message,
            StepPointsEarned = stepPointsEarned,
            TaskPointsEarned = taskPointsEarned,
            CurrentTotalPoints = user.TotalPoints,
            TotalSteps = totalSteps,
            StreakSecured = streakSecured,
            GoalsUpdated = goalsResult?.UpdatedCount ?? 0,
            GoalsCompleted = goalsResult?.CompletedCount ?? 0,
            TasksUpdated = tasksResult?.UpdatedCount ?? 0,
            TasksCompleted = tasksResult?.CompletedCount ?? 0,
            DuelsUpdated = duelsResult?.UpdatedCount ?? 0,
            DuelsFinished = duelsResult?.CompletedCount ?? 0,
            PartnerMissionsUpdated = partnerResult?.UpdatedCount ?? 0,
            GlobalMissionsContributed = globalResult?.UpdatedCount ?? 0,
            AlreadyProcessed = stepsAlreadyProcessed,
            RejectedActivities = rejectedCount
        };
    }

    private static string BuildResponseMessage(
        int totalSteps, int stepPoints, int taskPoints, bool streakSecured,
        int goalsCompleted, int tasksCompleted, int duelsFinished)
    {
        var parts = new List<string>();

        if (totalSteps > 0)
        {
            parts.Add($"{totalSteps:N0} adım");
        }

        int totalPoints = stepPoints + taskPoints;
        if (totalPoints > 0)
        {
            parts.Add($"{totalPoints} puan kazanıldı");
        }

        if (streakSecured)
        {
            parts.Add("streak korundu");
        }
        else if (totalSteps > 0)
        {
            int remaining = 3000 - totalSteps;
            parts.Add($"streak için {remaining:N0} adım daha gerekli");
        }

        if (goalsCompleted > 0)
        {
            parts.Add($"{goalsCompleted} hedef tamamlandı");
        }

        if (tasksCompleted > 0)
        {
            parts.Add($"{tasksCompleted} görev tamamlandı");
        }

        if (duelsFinished > 0)
        {
            parts.Add($"{duelsFinished} düello sonuçlandı");
        }

        return parts.Count > 0
            ? string.Join(", ", parts) + "."
            : "Sağlık verileri senkronize edildi.";
    }
}
