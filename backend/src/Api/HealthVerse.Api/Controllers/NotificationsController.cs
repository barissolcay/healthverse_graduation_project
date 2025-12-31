using HealthVerse.Notifications.Application.Commands;
using HealthVerse.Notifications.Application.DTOs;
using HealthVerse.Notifications.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IMediator mediator,
        ICurrentUser currentUser,
        ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Bildirim listesi (sayfalı).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NotificationsListResponse>> GetNotifications(
        int page = 1,
        int pageSize = 20,
        bool unreadOnly = false)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(_currentUser.UserId, page, pageSize, unreadOnly));
        return Ok(result);
    }

    /// <summary>
    /// Okunmamış bildirim sayısı.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountResponse>> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadNotificationCountQuery(_currentUser.UserId));
        return Ok(result);
    }

    /// <summary>
    /// Bildirimleri okundu olarak işaretle.
    /// </summary>
    [HttpPost("mark-read")]
    public async Task<ActionResult<NotificationActionResponse>> MarkAsRead([FromBody] MarkReadRequest request)
    {
        var ids = request.NotificationIds ?? new List<Guid>();
        var result = await _mediator.Send(new MarkNotificationReadCommand(ids, request.All, _currentUser.UserId));
        return Ok(result);
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretle.
    /// </summary>
    [HttpPost("clear-all")]
    public async Task<ActionResult<NotificationActionResponse>> ClearAll()
    {
        var result = await _mediator.Send(new ClearAllNotificationsCommand(_currentUser.UserId));
        return Ok(result);
    }

    // ===== Notification Preferences =====

    /// <summary>
    /// Kullanıcının bildirim tercihlerini getir.
    /// Tüm kategoriler için tercihler döner (kayıtlı veya default).
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferencesResponse>> GetPreferences()
    {
        var result = await _mediator.Send(new GetNotificationPreferencesQuery(_currentUser.UserId));
        return Ok(result);
    }

    /// <summary>
    /// Bildirim tercihlerini güncelle.
    /// Sadece gönderilen kategoriler güncellenir.
    /// </summary>
    [HttpPut("preferences")]
    public async Task<ActionResult<UpdatePreferencesResponse>> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request)
    {
        var result = await _mediator.Send(new UpdateNotificationPreferencesCommand(
            _currentUser.UserId,
            request.Preferences));
        return Ok(result);
    }
}
