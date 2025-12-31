using HealthVerse.Identity.Application.DTOs;
using HealthVerse.Identity.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Application.Commands;

/// <summary>
/// [DEV ONLY] Login test user without Firebase authentication.
/// </summary>
public sealed record DevLoginCommand(string Email) : IRequest<DevLoginResponse>;

public sealed class DevLoginCommandHandler : IRequestHandler<DevLoginCommand, DevLoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthIdentityRepository _authIdentityRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly ILogger<DevLoginCommandHandler> _logger;

    public DevLoginCommandHandler(
        IUserRepository userRepository,
        IAuthIdentityRepository authIdentityRepository,
        IIdentityUnitOfWork unitOfWork,
        ILogger<DevLoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _authIdentityRepository = authIdentityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DevLoginResponse> Handle(DevLoginCommand request, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new DevLoginResponse
            {
                Success = false,
                Message = "Email gerekli."
            };
        }

        // Find auth identity by email
        var authIdentity = await _authIdentityRepository.GetByEmailAsync(request.Email, ct);
        if (authIdentity == null)
        {
            return new DevLoginResponse
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
            return new DevLoginResponse
            {
                Success = false,
                Message = "Kullanıcı profili bulunamadı."
            };
        }

        // Update last login
        authIdentity.UpdateLastLogin();
        await _unitOfWork.SaveChangesAsync(ct);

        return new DevLoginResponse
        {
            Success = true,
            Message = "Giriş başarılı (DEV).",
            UserId = user.Id,
            Username = user.Username.Value,
            IsNewUser = false
        };
    }
}
