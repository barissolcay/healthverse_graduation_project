namespace HealthVerse.Social.Domain.Entities;

/// <summary>
/// Kullanıcı engelleme ilişkisini temsil eder.
/// Composite Primary Key: (BlockerId, BlockedId)
/// Kendini engelleme yasaktır (constraint).
/// 
/// Engelleme durumunda:
/// - Engellenen kişi engelleyeni aramalarda göremez
/// - Düello/Partner isteği atılamaz
/// - Mevcut takip ilişkisi backend tarafından silinir
/// </summary>
public sealed class UserBlock
{
    /// <summary>
    /// Engelleyen kullanıcının ID'si.
    /// </summary>
    public Guid BlockerId { get; private init; }

    /// <summary>
    /// Engellenen kullanıcının ID'si.
    /// </summary>
    public Guid BlockedId { get; private init; }

    /// <summary>
    /// Engelleme zamanı.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }

    private UserBlock() { }

    public static UserBlock Create(Guid blockerId, Guid blockedId)
    {
        if (blockerId == Guid.Empty)
            throw new ArgumentException("BlockerId cannot be empty.", nameof(blockerId));

        if (blockedId == Guid.Empty)
            throw new ArgumentException("BlockedId cannot be empty.", nameof(blockedId));

        if (blockerId == blockedId)
            throw new InvalidOperationException("A user cannot block themselves.");

        return new UserBlock
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
