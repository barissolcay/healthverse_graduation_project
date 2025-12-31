using HealthVerse.Notifications.Application.DTOs;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using MediatR;

namespace HealthVerse.Notifications.Application.Commands;

// --- Commands ---

public sealed record MarkNotificationReadCommand(List<Guid> NotificationIds, bool MarkAll, Guid UserId) : IRequest<NotificationActionResponse>;
public sealed record ClearAllNotificationsCommand(Guid UserId) : IRequest<NotificationActionResponse>;
public sealed record RegisterDeviceCommand(string PushToken, string Platform, string DeviceModel, string AppVersion, Guid UserId) : IRequest<RegisterDeviceResponse>;
public sealed record UnregisterDeviceCommand(string PushToken, Guid UserId) : IRequest<DeleteDeviceResponse>;

// --- Handlers ---

public sealed class NotificationCommandHandlers :
    IRequestHandler<MarkNotificationReadCommand, NotificationActionResponse>,
    IRequestHandler<ClearAllNotificationsCommand, NotificationActionResponse>,
    IRequestHandler<RegisterDeviceCommand, RegisterDeviceResponse>,
    IRequestHandler<UnregisterDeviceCommand, DeleteDeviceResponse>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IUserDeviceRepository _deviceRepo;
    private readonly INotificationsUnitOfWork _unitOfWork;

    public NotificationCommandHandlers(
        INotificationRepository notificationRepo,
        IUserDeviceRepository deviceRepo,
        INotificationsUnitOfWork unitOfWork)
    {
        _notificationRepo = notificationRepo;
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationActionResponse> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        List<Notification> notificationsToMark;

        if (request.MarkAll)
        {
            notificationsToMark = await _notificationRepo.GetUnreadByUserAsync(request.UserId, ct);
        }
        else
        {
            // Only unread ones that match IDs
            var allByIds = await _notificationRepo.GetByIdsAsync(request.NotificationIds, ct);
            notificationsToMark = allByIds.Where(n => n.UserId == request.UserId && !n.IsRead).ToList();
        }

        foreach (var notification in notificationsToMark)
        {
            notification.MarkAsRead();
        }

        if (notificationsToMark.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return new NotificationActionResponse
        {
            Success = true,
            Message = $"{notificationsToMark.Count} bildirim okundu olarak işaretlendi.",
            AffectedCount = notificationsToMark.Count
        };
    }

    public async Task<NotificationActionResponse> Handle(ClearAllNotificationsCommand request, CancellationToken ct)
    {
        var unread = await _notificationRepo.GetUnreadByUserAsync(request.UserId, ct);
        foreach (var n in unread)
        {
            n.MarkAsRead();
        }

        if (unread.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return new NotificationActionResponse
        {
            Success = true,
            Message = "Tüm bildirimler okundu olarak işaretlendi.",
            AffectedCount = unread.Count
        };
    }

    public async Task<RegisterDeviceResponse> Handle(RegisterDeviceCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PushToken))
             return new RegisterDeviceResponse { Success = false, Message = "PushToken boş olamaz." };

        var existingDevice = await _deviceRepo.GetByTokenAsync(request.PushToken, ct);

        if (existingDevice != null)
        {
            if (existingDevice.UserId != request.UserId)
            {
                await _deviceRepo.RemoveAsync(existingDevice, ct);
                // Create new since owner changed
            }
            else
            {
                existingDevice.UpdateLastActive();
                await _unitOfWork.SaveChangesAsync(ct);
                return new RegisterDeviceResponse { Success = true, Message = "Cihaz güncellendi.", DeviceId = existingDevice.Id };
            }
        }

        var newDevice = UserDevice.Create(request.UserId, request.PushToken, request.Platform, request.DeviceModel, request.AppVersion);
        await _deviceRepo.AddAsync(newDevice, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new RegisterDeviceResponse { Success = true, Message = "Cihaz kaydedildi.", DeviceId = newDevice.Id };
    }

    public async Task<DeleteDeviceResponse> Handle(UnregisterDeviceCommand request, CancellationToken ct)
    {
         var device = await _deviceRepo.GetByTokenAsync(request.PushToken, ct);
         if (device == null || device.UserId != request.UserId)
         {
             return new DeleteDeviceResponse { Success = false, Message = "Cihaz bulunamadı." };
         }

         await _deviceRepo.RemoveAsync(device, ct);
         await _unitOfWork.SaveChangesAsync(ct);

         return new DeleteDeviceResponse { Success = true, Message = "Cihaz silindi." };
    }
}
