using HealthVerse.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Application.EventHandlers;

/// <summary>
/// Handler for UserCreatedEvent.
/// This is an example of how to handle domain events.
/// Can be extended to send welcome notifications, initialize user data, etc.
/// </summary>
public sealed class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User created: {UserId} ({Username}, {Email})",
            notification.UserId,
            notification.Username,
            notification.Email);

        // Future enhancements:
        // - Send welcome notification
        // - Initialize default user settings
        // - Create initial gamification data
        // - Track analytics event

        return Task.CompletedTask;
    }
}
