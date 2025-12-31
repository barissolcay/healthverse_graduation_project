namespace HealthVerse.Identity.Application.DTOs;

// ===== Auth Request DTOs =====

/// <summary>
/// Kayıt request (Firebase token ile).
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// Firebase ID token (client'tan alınır).
    /// </summary>
    public string IdToken { get; init; } = null!;
    
    /// <summary>
    /// Kullanıcı adı (benzersiz olmalı).
    /// </summary>
    public string Username { get; init; } = null!;
    
    /// <summary>
    /// Seçilen avatar ID.
    /// </summary>
    public int AvatarId { get; init; } = 1;
}

/// <summary>
/// Kayıt response.
/// </summary>
public sealed class RegisterResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? UserId { get; init; }
    public string? Username { get; init; }
}

/// <summary>
/// Login request (Firebase token ile).
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// Firebase ID token (client'tan alınır).
    /// </summary>
    public string IdToken { get; init; } = null!;
}

/// <summary>
/// Login response.
/// </summary>
public sealed class LoginResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? UserId { get; init; }
    public string? Username { get; init; }
    public bool IsNewUser { get; init; }
}

/// <summary>
/// Mevcut kullanıcı bilgisi.
/// </summary>
public sealed class CurrentUserResponse
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public int AvatarId { get; init; }
    public string? City { get; init; }
    public string? Bio { get; init; }
    public long TotalPoints { get; init; }
    public int StreakCount { get; init; }
    public int FreezeInventory { get; init; }
    public string CurrentTier { get; init; } = null!;
    public string? SelectedTitleId { get; init; }
}

// ===== Development-Only DTOs =====

/// <summary>
/// [DEV ONLY] Test kayıt request.
/// </summary>
public sealed class DevRegisterRequest
{
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public int AvatarId { get; init; } = 1;
}

/// <summary>
/// [DEV ONLY] Test login request.
/// </summary>
public sealed class DevLoginRequest
{
    public string Email { get; init; } = null!;
}

/// <summary>
/// [DEV ONLY] Test login response.
/// </summary>
public sealed class DevLoginResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? UserId { get; init; }
    public string? Username { get; init; }
    public bool IsNewUser { get; init; }
}

