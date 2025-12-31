using HealthVerse.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Application.EventHandlers;

/// <summary>
/// Handler for StreakLostEvent.
/// Logs streak loss and can trigger recovery notifications.
/// </summary>
public sealed class StreakLostEventHandler : INotificationHandler<StreakLostEvent>
{
    private readonly ILogger<StreakLostEventHandler> _logger;

    public StreakLostEventHandler(ILogger<StreakLostEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(StreakLostEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "User {UserId} lost streak. Previous streak: {LostStreakCount} days",
            notification.UserId,
            notification.LostStreakCount);

        // Future enhancements:
        // - Send motivational notification
        // - Offer streak freeze purchase
        // - Track analytics for churn prevention

        return Task.CompletedTask;
    }
}
