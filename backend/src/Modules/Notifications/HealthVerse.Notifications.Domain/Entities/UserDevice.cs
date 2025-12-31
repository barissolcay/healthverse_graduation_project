using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Notifications.Domain.Entities;

/// <summary>
/// Kullanıcı cihaz bilgisi - push notification token.
/// </summary>
public sealed class UserDevice : Entity
{
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// FCM veya APNS token.
    /// </summary>
    public string PushToken { get; private init; } = null!;
    
    /// <summary>
    /// Platform: IOS, ANDROID
    /// </summary>
    public string Platform { get; private init; } = null!;
    
    /// <summary>
    /// Cihaz modeli (opsiyonel).
    /// </summary>
    public string? DeviceModel { get; private init; }
    
    /// <summary>
    /// Uygulama versiyonu.
    /// </summary>
    public string? AppVersion { get; private init; }
    
    /// <summary>
    /// Token'ın ne zaman kaydedildiği.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }
    
    /// <summary>
    /// Son aktivite zamanı (token güncelleme).
    /// </summary>
    public DateTimeOffset LastActiveAt { get; private set; }

    /// <summary>
    /// Device is active and can receive push notifications.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    private UserDevice() { }

    public static UserDevice Create(
        Guid userId,
        string pushToken,
        string platform,
        string? deviceModel = null,
        string? appVersion = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserDevice.InvalidUser", "UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(pushToken))
            throw new DomainException("UserDevice.InvalidToken", "PushToken cannot be empty.");

        if (string.IsNullOrWhiteSpace(platform))
            throw new DomainException("UserDevice.InvalidPlatform", "Platform cannot be empty.");

        var now = DateTimeOffset.UtcNow;
        return new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PushToken = pushToken,
            Platform = platform.ToUpperInvariant(),
            DeviceModel = deviceModel,
            AppVersion = appVersion,
            CreatedAt = now,
            LastActiveAt = now
        };
    }

    /// <summary>
    /// Son aktiviteyi güncelle.
    /// </summary>
    public void UpdateLastActive()
    {
        LastActiveAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Disable the device (invalid token).
    /// </summary>
    public void Disable()
    {
        IsActive = false;
    }

    /// <summary>
    /// Re-enable the device.
    /// </summary>
    public void Enable()
    {
        IsActive = true;
    }
}

/// <summary>
/// Platform sabitleri.
/// </summary>
public static class DevicePlatform
{
    public const string IOS = "IOS";
    public const string ANDROID = "ANDROID";
}
