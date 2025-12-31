using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Notifications.Domain.Entities;

/// <summary>
/// Kullanıcının bildirim tercihleri.
/// Her kullanıcı her kategori için push tercihini ve sessiz saatlerini ayarlayabilir.
/// Kayıt yoksa kategori için default policy uygulanır.
/// </summary>
public sealed class UserNotificationPreference : Entity
{
    public Guid UserId { get; private init; }
    public NotificationCategory Category { get; private init; }
    
    /// <summary>
    /// Bu kategori için push bildirimleri açık mı?
    /// Default: Kategoriye göre değişir (Goal ve Milestone default kapalı, diğerleri açık)
    /// </summary>
    public bool PushEnabled { get; private set; }
    
    /// <summary>
    /// Sessiz saat başlangıcı (UTC).
    /// Null ise sessiz saat yok.
    /// Örn: 22:00 (gece 10)
    /// </summary>
    public TimeOnly? QuietHoursStart { get; private set; }
    
    /// <summary>
    /// Sessiz saat bitişi (UTC).
    /// Null ise sessiz saat yok.
    /// Örn: 08:00 (sabah 8)
    /// </summary>
    public TimeOnly? QuietHoursEnd { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private UserNotificationPreference() { }

    /// <summary>
    /// Yeni bir tercih kaydı oluşturur.
    /// </summary>
    public static UserNotificationPreference Create(
        Guid userId,
        NotificationCategory category,
        bool pushEnabled,
        TimeOnly? quietHoursStart = null,
        TimeOnly? quietHoursEnd = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserNotificationPreference.InvalidUser", "UserId cannot be empty.");

        // Quiet hours validasyonu: ikisi de set ya da ikisi de null olmalı
        if ((quietHoursStart.HasValue && !quietHoursEnd.HasValue) ||
            (!quietHoursStart.HasValue && quietHoursEnd.HasValue))
        {
            throw new DomainException(
                "UserNotificationPreference.InvalidQuietHours",
                "QuietHoursStart and QuietHoursEnd must both be set or both be null.");
        }

        var now = DateTimeOffset.UtcNow;
        return new UserNotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = category,
            PushEnabled = pushEnabled,
            QuietHoursStart = quietHoursStart,
            QuietHoursEnd = quietHoursEnd,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Default tercihlerle yeni bir kayıt oluşturur.
    /// Push enabled değeri kategoriye göre belirlenir.
    /// </summary>
    public static UserNotificationPreference CreateWithDefaults(
        Guid userId,
        NotificationCategory category)
    {
        return Create(
            userId,
            category,
            category is not (NotificationCategory.Goal or NotificationCategory.Milestone));
    }

    /// <summary>
    /// Push enabled durumunu günceller.
    /// </summary>
    public void SetPushEnabled(bool enabled)
    {
        PushEnabled = enabled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sessiz saatleri günceller.
    /// </summary>
    public void SetQuietHours(TimeOnly? start, TimeOnly? end)
    {
        if ((start.HasValue && !end.HasValue) ||
            (!start.HasValue && end.HasValue))
        {
            throw new DomainException(
                "UserNotificationPreference.InvalidQuietHours",
                "QuietHoursStart and QuietHoursEnd must both be set or both be null.");
        }

        QuietHoursStart = start;
        QuietHoursEnd = end;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sessiz saatleri temizler.
    /// </summary>
    public void ClearQuietHours()
    {
        QuietHoursStart = null;
        QuietHoursEnd = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Verilen zamanda sessiz saatler içinde mi kontrol eder.
    /// </summary>
    public bool IsInQuietHours(TimeOnly currentTime)
    {
        if (!QuietHoursStart.HasValue || !QuietHoursEnd.HasValue)
            return false;

        var start = QuietHoursStart.Value;
        var end = QuietHoursEnd.Value;

        // Gece yarısını geçen sessiz saatler (örn: 22:00 - 08:00)
        if (start > end)
        {
            return currentTime >= start || currentTime <= end;
        }

        // Normal sessiz saatler (örn: 13:00 - 14:00)
        return currentTime >= start && currentTime <= end;
    }
}
