namespace HealthVerse.Tasks.Application.DTOs;

// ===== Task DTOs =====

/// <summary>
/// Görev detay bilgisi.
/// </summary>
public sealed class TaskDetailDto
{
    public Guid Id { get; init; }
    public Guid TemplateId { get; init; }
    
    // Template bilgileri
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string? ActivityType { get; init; }
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public int RewardPoints { get; init; }
    public string? BadgeId { get; init; }
    public string? TitleId { get; init; }
    
    // Progress
    public int CurrentValue { get; init; }
    public int ProgressPercent { get; init; }
    
    // Status
    public string Status { get; init; } = null!;
    public DateTimeOffset ValidUntil { get; init; }
    public DateTimeOffset AssignedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? RewardClaimedAt { get; init; }
    
    /// <summary>
    /// Kalan süre (saat).
    /// </summary>
    public int HoursRemaining { get; init; }
}

/// <summary>
/// Aktif görevler listesi response.
/// </summary>
public sealed class ActiveTasksResponse
{
    public List<TaskDetailDto> Tasks { get; init; } = new();
    public int TotalActive { get; init; }
}

/// <summary>
/// Tamamlanan görevler listesi response.
/// </summary>
public sealed class CompletedTasksResponse
{
    public List<TaskDetailDto> Tasks { get; init; } = new();
    public int TotalCompleted { get; init; }
}

/// <summary>
/// Ödül toplama response.
/// </summary>
public sealed class ClaimRewardResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public int? PointsEarned { get; init; }
    public string? BadgeEarned { get; init; }
    public string? TitleEarned { get; init; }
}

/// <summary>
/// Görev şablonu (admin için).
/// </summary>
public sealed class TaskTemplateDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string? ActivityType { get; init; }
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public int RewardPoints { get; init; }
    public string? BadgeId { get; init; }
    public string? TitleId { get; init; }
    public bool IsActive { get; init; }
}

// ===== Goal DTOs =====

/// <summary>
/// Hedef detay bilgisi.
/// </summary>
public sealed class GoalDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string? ActivityType { get; init; }
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public int CurrentValue { get; init; }
    public int ProgressPercent { get; init; }
    public DateTimeOffset ValidUntil { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public int HoursRemaining { get; init; }
    public bool IsCompleted { get; init; }
}

/// <summary>
/// Yeni hedef oluşturma request.
/// </summary>
public sealed class CreateGoalRequest
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string? ActivityType { get; init; }
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public DateTimeOffset ValidUntil { get; init; }
}

/// <summary>
/// Hedef oluşturma response.
/// </summary>
public sealed class CreateGoalResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? GoalId { get; init; }
}

/// <summary>
/// Aktif hedefler listesi response.
/// </summary>
public sealed class ActiveGoalsResponse
{
    public List<GoalDetailDto> Goals { get; init; } = new();
    public int TotalActive { get; init; }
}

/// <summary>
/// Tamamlanan hedefler listesi response.
/// </summary>
public sealed class CompletedGoalsResponse
{
    public List<GoalDetailDto> Goals { get; init; } = new();
    public int TotalCompleted { get; init; }
}

// ===== Interest DTOs =====

/// <summary>
/// İlgi alanı kaydetme request.
/// </summary>
public sealed class SaveInterestsRequest
{
    public List<string> ActivityTypes { get; init; } = new();
}

/// <summary>
/// İlgi alanları response.
/// </summary>
public sealed class InterestsResponse
{
    public List<string> ActivityTypes { get; init; } = new();
    public int TotalInterests { get; init; }
}
