using System.Security.Claims;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HealthVerse.Infrastructure.Auth;

public class CurrentUserAdapter : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAdapter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("Kullanıcı doğrulanmadı (unauthenticated request).");
            }

            var userIdClaim = user.FindFirst("user_id")?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Kullanıcı kimliği (user_id) claim'i eksik veya geçersiz.");
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
