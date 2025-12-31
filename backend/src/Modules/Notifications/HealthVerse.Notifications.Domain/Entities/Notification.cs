using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Notifications.Domain.Entities;

/// <summary>
/// In-app bildirim entity.
/// Kullanıcının bildirim kutusunda görünen mesajlar.
/// </summary>
public sealed class Notification : Entity
{
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Bildirim tipi: STREAK_FROZEN, DUEL_REQUEST, MISSION_COMPLETE, vb.
    /// </summary>
    public string Type { get; private init; } = null!;
    
    /// <summary>
    /// Bildirim başlığı.
    /// </summary>
    public string Title { get; private init; } = null!;
    
    /// <summary>
    /// Bildirim içeriği.
    /// </summary>
    public string Body { get; private init; } = null!;
    
    /// <summary>
    /// İlgili entity ID (DuelId, MissionId, vb.).
    /// </summary>
    public Guid? ReferenceId { get; private init; }
    
    /// <summary>
    /// İlgili entity tipi: DUEL, MISSION, TASK, vb.
    /// </summary>
    public string? ReferenceType { get; private init; }
    
    /// <summary>
    /// Ekstra veri (JSON).
    /// </summary>
    public string? Data { get; private init; }
    
    public bool IsRead { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? referenceId = null,
        string? referenceType = null,
        string? data = null,
        DateTimeOffset? createdAt = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Notification.InvalidUser", "UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(type))
            throw new DomainException("Notification.InvalidType", "Type cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Notification.InvalidTitle", "Title cannot be empty.");

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body ?? string.Empty,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Data = data,
            IsRead = false,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Okundu olarak işaretle.
    /// </summary>
    /// <param name="readAt">Okunma zamanı (test için IClock üzerinden geçirilebilir)</param>
    public void MarkAsRead(DateTimeOffset? readAt = null)
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = readAt ?? DateTimeOffset.UtcNow;
    }
}

