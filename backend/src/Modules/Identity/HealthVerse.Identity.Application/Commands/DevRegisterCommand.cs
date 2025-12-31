using HealthVerse.Contracts.Notifications;
using HealthVerse.Identity.Application.DTOs;
using HealthVerse.Identity.Application.Ports;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Application.Commands;

/// <summary>
/// [DEV ONLY] Register a test user without Firebase authentication.
/// </summary>
public sealed record DevRegisterCommand(
    string Username,
    string Email,
    int AvatarId = 1
) : IRequest<RegisterResponse>;

public sealed class DevRegisterCommandHandler : IRequestHandler<DevRegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthIdentityRepository _authIdentityRepository;
    private readonly INotificationService _notificationService;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly ILogger<DevRegisterCommandHandler> _logger;

    public DevRegisterCommandHandler(
        IUserRepository userRepository,
        IAuthIdentityRepository authIdentityRepository,
        INotificationService notificationService,
        IIdentityUnitOfWork unitOfWork,
        ILogger<DevRegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _authIdentityRepository = authIdentityRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(DevRegisterCommand request, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Username ve Email gerekli."
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

        // Create fake Firebase UID
        var fakeFirebaseUid = $"dev_{Guid.NewGuid()}";

        // Create user
        var username = Username.Create(request.Username);
        var email = Email.Create(request.Email);
        var user = User.Create(username, email, request.AvatarId);

        await _userRepository.AddAsync(user, ct);

        // Create auth identity
        var authIdentity = AuthIdentity.Create(
            fakeFirebaseUid,
            user.Id,
            "DEV",
            request.Email
        );

        await _authIdentityRepository.AddAsync(authIdentity, ct);

        // Create welcome notification with push delivery
        await _notificationService.CreateAsync(
            user.Id,
            NotificationType.WELCOME,
            "HealthVerse'e hoş geldin! 🎉",
            $"Merhaba {user.Username.Value}! Sağlıklı yaşam yolculuğuna hoş geldin.",
            ct: ct
        );

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("[DEV] Test kullanıcı kaydı: {UserId}, {Username}", user.Id, request.Username);

        return new RegisterResponse
        {
            Success = true,
            Message = "Kayıt başarılı (DEV).",
            UserId = user.Id,
            Username = user.Username.Value
        };
    }
}
