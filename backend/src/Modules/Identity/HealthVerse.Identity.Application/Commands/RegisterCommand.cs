using HealthVerse.Contracts.Notifications;
using HealthVerse.Identity.Application.DTOs;
using HealthVerse.Identity.Application.Ports;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Application.Commands;

/// <summary>
/// Register a new user with Firebase authentication.
/// </summary>
public sealed record RegisterCommand(
    string IdToken,
    string Username,
    int AvatarId = 1
) : IRequest<RegisterResponse>;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IFirebaseAuthService _firebaseAuth;
    private readonly IUserRepository _userRepository;
    private readonly IAuthIdentityRepository _authIdentityRepository;
    private readonly INotificationService _notificationService;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IFirebaseAuthService firebaseAuth,
        IUserRepository userRepository,
        IAuthIdentityRepository authIdentityRepository,
        INotificationService notificationService,
        IIdentityUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _firebaseAuth = firebaseAuth;
        _userRepository = userRepository;
        _authIdentityRepository = authIdentityRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Firebase ID token gerekli."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Kullanıcı adı gerekli."
            };
        }

        // Verify Firebase token
        var firebaseUser = await _firebaseAuth.VerifyIdTokenAsync(request.IdToken, ct);
        if (firebaseUser == null)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Geçersiz veya süresi dolmuş token."
            };
        }

        // Check if Firebase UID already registered
        var existingAuth = await _authIdentityRepository.GetByFirebaseUidAsync(firebaseUser.Uid, ct);
        if (existingAuth != null)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Bu hesap zaten kayıtlı. Giriş yapmayı deneyin."
            };
        }

        // Check if username is taken
        if (await _userRepository.IsUsernameTakenAsync(request.Username, ct))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Bu kullanıcı adı zaten kullanılıyor."
            };
        }

        // Create user
        var username = Username.Create(request.Username);
        var email = Email.Create(firebaseUser.Email);
        var user = User.Create(username, email, request.AvatarId);

        await _userRepository.AddAsync(user, ct);

        // Create auth identity
        var authIdentity = AuthIdentity.Create(
            firebaseUser.Uid,
            user.Id,
            firebaseUser.Provider,
            firebaseUser.Email
        );

        await _authIdentityRepository.AddAsync(authIdentity, ct);

        // Create welcome notification with push delivery
        await _notificationService.CreateAsync(
            user.Id,
            NotificationType.WELCOME,
            "HealthVerse'e hoş geldin! 🎉",
            $"Merhaba {user.Username.Value}! Sağlıklı yaşam yolculuğuna hoş geldin. İlk görevini tamamlayarak hemen puan kazanmaya başla!",
            ct: ct
        );

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Yeni kullanıcı kaydı: {UserId}, {Username}", user.Id, request.Username);

        return new RegisterResponse
        {
            Success = true,
            Message = "Kayıt başarılı.",
            UserId = user.Id,
            Username = user.Username.Value
        };
    }
}
