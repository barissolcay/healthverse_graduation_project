using HealthVerse.Contracts.Notifications;
using HealthVerse.Notifications.Application;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Infrastructure.Persistence;
using HealthVerse.Notifications.Infrastructure.Push;
using HealthVerse.Notifications.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Notifications.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Notifications module infrastructure services (repositories, ports).
    /// </summary>
    public static IServiceCollection AddNotificationsInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
        services.AddScoped<IUserNotificationPreferenceRepository, UserNotificationPreferenceRepository>();

        // Push Sender
        services.AddScoped<IPushSender, FirebasePushSender>();

        // Push Policy (decides whether to send push based on category + user preference)
        services.AddScoped<INotificationPushPolicy, NotificationPushPolicy>();

        // Notification Service - register as both cross-module contract and internal interface
        services.AddScoped<NotificationService>();
        services.AddScoped<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
        services.AddScoped<INotificationServiceInternal>(sp => sp.GetRequiredService<NotificationService>());

        // Unit of Work
        services.AddScoped<INotificationsUnitOfWork, NotificationsUnitOfWork>();

        services.AddNotificationsApplication();

        return services;
    }
}
