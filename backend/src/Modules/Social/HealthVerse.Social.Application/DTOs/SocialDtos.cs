namespace HealthVerse.Social.Application.DTOs;

/// <summary>
/// Kullanıcı özet bilgisi (listeler için).
/// Takipçiler, takip edilenler, arkadaşlar listelerinde kullanılır.
/// </summary>
public sealed class UserSummaryDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = null!;
    public int AvatarId { get; init; }
    public string? SelectedTitleId { get; init; }
    public long TotalPoints { get; init; }
    public string CurrentTier { get; init; } = null!;
}

/// <summary>
/// Takip işlemi sonucu.
/// </summary>
public sealed class FollowResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    
    /// <summary>
    /// İşlem sonrası toplam takip edilen sayısı.
    /// </summary>
    public int FollowingCount { get; init; }
    
    /// <summary>
    /// Karşılıklı takip var mı? (Artık arkadaş mı?)
    /// </summary>
    public bool IsMutual { get; init; }
}

/// <summary>
/// Takip listesi sonucu (sayfalı).
/// </summary>
public sealed class FollowListResponse
{
    public List<UserSummaryDto> Users { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

/// <summary>
/// Engelleme işlemi sonucu.
/// </summary>
public sealed class BlockResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}
