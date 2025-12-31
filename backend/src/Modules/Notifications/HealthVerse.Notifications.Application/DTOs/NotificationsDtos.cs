namespace HealthVerse.Notifications.Application.DTOs;

// ===== Notification DTOs =====

/// <summary>
/// Bildirim detay bilgisi.
/// </summary>
public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Body { get; init; } = null!;
    public Guid? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public string? Data { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}

/// <summary>
/// Bildirim listesi response.
/// </summary>
public sealed class NotificationsListResponse
{
    public List<NotificationDto> Notifications { get; init; } = new();
    public int TotalCount { get; init; }
    public int UnreadCount { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>
/// Okunmamış sayı response.
/// </summary>
public sealed class UnreadCountResponse
{
    public int Count { get; init; }
}

/// <summary>
/// Bildirimleri okundu işaretleme request.
/// </summary>
public sealed class MarkReadRequest
{
    public List<Guid>? NotificationIds { get; init; }
    public bool All { get; init; }
}

/// <summary>
/// Bildirim action response.
/// </summary>
public sealed class NotificationActionResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public int AffectedCount { get; init; }
}

// ===== Device DTOs =====

/// <summary>
/// Cihaz kayıt request.
/// </summary>
public sealed class RegisterDeviceRequest
{
    public string PushToken { get; init; } = null!;
    public string Platform { get; init; } = null!;
    public string? DeviceModel { get; init; }
    public string? AppVersion { get; init; }
}

/// <summary>
/// Cihaz kayıt response.
/// </summary>
public sealed class RegisterDeviceResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? DeviceId { get; init; }
}

/// <summary>
/// Cihaz silme response.
/// </summary>
public sealed class DeleteDeviceResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

// ===== Notification Preference DTOs =====

/// <summary>
/// Tek bir kategori için bildirim tercihi.
/// </summary>
public sealed class NotificationPreferenceDto
{
    /// <summary>
    /// Kategori adı (Streak, Duel, Task, Goal, PartnerMission, GlobalMission, League, Social, Milestone, System)
    /// </summary>
    public string Category { get; init; } = null!;
    
    /// <summary>
    /// Kategori görünen adı (UI için)
    /// </summary>
    public string DisplayName { get; init; } = null!;
    
    /// <summary>
    /// Push bildirimleri açık mı?
    /// </summary>
    public bool PushEnabled { get; init; }
    
    /// <summary>
    /// Sessiz saat başlangıcı (HH:mm formatında, UTC). Null ise sessiz saat yok.
    /// </summary>
    public string? QuietHoursStart { get; init; }
    
    /// <summary>
    /// Sessiz saat bitişi (HH:mm formatında, UTC). Null ise sessiz saat yok.
    /// </summary>
    public string? QuietHoursEnd { get; init; }
}

/// <summary>
/// Kullanıcının tüm bildirim tercihlerini dönen response.
/// </summary>
public sealed class NotificationPreferencesResponse
{
    /// <summary>
    /// Tüm kategoriler için tercihler.
    /// Kayıt olmayan kategoriler için default değerler döner.
    /// </summary>
    public List<NotificationPreferenceDto> Preferences { get; init; } = new();
}

/// <summary>
/// Bildirim tercihlerini güncelleme request.
/// </summary>
public sealed class UpdateNotificationPreferencesRequest
{
    /// <summary>
    /// Güncellenecek tercihler listesi.
    /// Sadece gönderilen kategoriler güncellenir.
    /// </summary>
    public List<UpdatePreferenceItem> Preferences { get; init; } = new();
}

/// <summary>
/// Tek bir kategori için güncelleme.
/// </summary>
public sealed class UpdatePreferenceItem
{
    /// <summary>
    /// Kategori adı (case-insensitive)
    /// </summary>
    public string Category { get; init; } = null!;
    
    /// <summary>
    /// Push bildirimleri açık mı?
    /// </summary>
    public bool PushEnabled { get; init; }
    
    /// <summary>
    /// Sessiz saat başlangıcı (HH:mm formatında, UTC). Null veya boş string sessiz saati kaldırır.
    /// </summary>
    public string? QuietHoursStart { get; init; }
    
    /// <summary>
    /// Sessiz saat bitişi (HH:mm formatında, UTC). Null veya boş string sessiz saati kaldırır.
    /// </summary>
    public string? QuietHoursEnd { get; init; }
}

/// <summary>
/// Tercih güncelleme response.
/// </summary>
public sealed class UpdatePreferencesResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public int UpdatedCount { get; init; }
}
