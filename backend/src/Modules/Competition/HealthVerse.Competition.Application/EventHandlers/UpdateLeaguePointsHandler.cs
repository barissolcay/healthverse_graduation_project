using HealthVerse.Competition.Application.Ports;
using HealthVerse.Contracts.Gamification;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Competition.Application.EventHandlers;

/// <summary>
/// Reacts to points earned in Gamification module to update League standings.
/// </summary>
public sealed class UpdateLeaguePointsHandler : INotificationHandler<UserPointsEarnedEvent>
{
    private readonly ILeagueMemberRepository _leagueMemberRepo;
    private readonly ICompetitionUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateLeaguePointsHandler> _logger;

    public UpdateLeaguePointsHandler(
        ILeagueMemberRepository leagueMemberRepo,
        ICompetitionUnitOfWork unitOfWork,
        ILogger<UpdateLeaguePointsHandler> logger)
    {
        _leagueMemberRepo = leagueMemberRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserPointsEarnedEvent notification, CancellationToken ct)
    {
        if (notification.PointsEarned <= 0)
            return;

        var weekId = WeekId.FromDate(notification.LogDate);
        
        // Find user's league membership for the current week
        var member = await _leagueMemberRepo.GetMembershipByUserAndWeekAsync(notification.UserId, weekId, ct);
        
        if (member == null)
        {
            // User hasn't joined a room this week yet, nothing to update.
            // Points will count when they join (if logic supports retroactive, but usually it starts from join).
            // Actually MVP: points only count if you are in a room.
            _logger.LogWarning("User {UserId} earned points but is not in a league room for week {WeekId}", notification.UserId, weekId.Value);
            return;
        }

        member.UpdatePoints(member.PointsInRoom + (int)notification.PointsEarned);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated League Points for User {UserId}: +{Points} (Total: {Total})", 
            notification.UserId, notification.PointsEarned, member.PointsInRoom);
    }
}
