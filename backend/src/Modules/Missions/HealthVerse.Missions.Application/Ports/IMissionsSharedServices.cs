namespace HealthVerse.Missions.Application.Ports;

/// <summary>
/// Service for accessing friendship information (Social module).
/// </summary>
public interface IFriendshipService
{
    Task<bool> IsMutualFriendAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
    Task<List<Guid>> GetMutualFriendIdsAsync(Guid userId, CancellationToken ct = default);
}

// Note: INotificationService removed - now using central INotificationService from HealthVerse.Notifications.Application.Ports

/// <summary>
/// Service for accessing basic user info (Identity module).
/// </summary>
public interface IMissionsUserService
{
    Task<Dictionary<Guid, (string Username, int AvatarId)>> GetUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task<(string Username, int AvatarId)?> GetUserAsync(Guid userId, CancellationToken ct = default);
}
