using HealthVerse.Notifications.Application.DTOs;
using HealthVerse.Notifications.Application.Ports;
using MediatR;

namespace HealthVerse.Notifications.Application.Queries;

// --- Queries ---

public sealed record GetNotificationsQuery(Guid UserId, int Page = 1, int PageSize = 20, bool UnreadOnly = false) : IRequest<NotificationsListResponse>;
public sealed record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<UnreadCountResponse>;

// --- Handlers ---

public sealed class NotificationQueryHandlers :
    IRequestHandler<GetNotificationsQuery, NotificationsListResponse>,
    IRequestHandler<GetUnreadNotificationCountQuery, UnreadCountResponse>
{
    private readonly INotificationRepository _notificationRepo;

    public NotificationQueryHandlers(INotificationRepository notificationRepo)
    {
        _notificationRepo = notificationRepo;
    }

    public async Task<NotificationsListResponse> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Min(Math.Max(request.PageSize, 5), 50);

        var notifications = await _notificationRepo.GetPaginatedByUserAsync(request.UserId, page, pageSize, request.UnreadOnly, ct);
        
        var hasMore = notifications.Count > pageSize;
        if (hasMore)
        {
            notifications = notifications.Take(pageSize).ToList();
        }

        var totalCount = await _notificationRepo.CountAsync(request.UserId, request.UnreadOnly, ct);
        var unreadCount = await _notificationRepo.GetUnreadCountAsync(request.UserId, ct);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Body = n.Body,
            ReferenceId = n.ReferenceId,
            ReferenceType = n.ReferenceType,
            Data = n.Data,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        }).ToList();

        return new NotificationsListResponse
        {
            Notifications = dtos,
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            HasMore = hasMore
        };
    }

    public async Task<UnreadCountResponse> Handle(GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var count = await _notificationRepo.GetUnreadCountAsync(request.UserId, ct);
        return new UnreadCountResponse { Count = count };
    }
}
