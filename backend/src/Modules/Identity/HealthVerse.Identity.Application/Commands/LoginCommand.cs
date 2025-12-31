using HealthVerse.Identity.Application.DTOs;
using HealthVerse.Identity.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Application.Commands;

/// <summary>
/// Login with Firebase authentication.
/// </summary>
public sealed record LoginCommand(string IdToken) : IRequest<LoginResponse>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IFirebaseAuthService _firebaseAuth;
    private readonly IUserRepository _userRepository;
    private readonly IAuthIdentityRepository _authIdentityRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IFirebaseAuthService firebaseAuth,
        IUserRepository userRepository,
        IAuthIdentityRepository authIdentityRepository,
        IIdentityUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger)
    {
        _firebaseAuth = firebaseAuth;
        _userRepository = userRepository;
        _authIdentityRepository = authIdentityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Firebase ID token gerekli."
            };
        }

        // Verify Firebase token
        var firebaseUser = await _firebaseAuth.VerifyIdTokenAsync(request.IdToken, ct);
        if (firebaseUser == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Geçersiz veya süresi dolmuş token."
            };
        }

        // Find auth identity
        var authIdentity = await _authIdentityRepository.GetByFirebaseUidAsync(firebaseUser.Uid, ct);
        if (authIdentity == null)
        {
            // User not registered
            return new LoginResponse
            {
                Success = true,
                Message = "Kullanıcı bulunamadı. Kayıt gerekli.",
                IsNewUser = true
            };
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(authIdentity.UserId, ct);
        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Kullanıcı profili bulunamadı."
            };
        }

        // Update last login
        authIdentity.UpdateLastLogin();
        await _unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse
        {
            Success = true,
            Message = "Giriş başarılı.",
            UserId = user.Id,
            Username = user.Username.Value,
            IsNewUser = false
        };
    }
}
