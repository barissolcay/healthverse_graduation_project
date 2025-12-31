using FirebaseAdmin.Auth;
using HealthVerse.Identity.Application.Ports;
using HealthVerse.Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Identity.Infrastructure.Auth;

/// <summary>
/// Firebase authentication service implementation.
/// </summary>
public sealed class FirebaseAuthService : IFirebaseAuthService
{
    private readonly ILogger<FirebaseAuthService> _logger;

    public FirebaseAuthService(ILogger<FirebaseAuthService> logger)
    {
        _logger = logger;
    }

    public async Task<FirebaseUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken, ct);
            var firebaseUid = decodedToken.Uid;
            var email = decodedToken.Claims.GetValueOrDefault("email")?.ToString() ?? "";
            var provider = GetProvider(decodedToken);

            return new FirebaseUserInfo(firebaseUid, email, provider);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Firebase token doğrulama hatası");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase token doğrulama sırasında beklenmeyen hata");
            return null;
        }
    }

    private static string GetProvider(FirebaseToken token)
    {
        if (token.Claims.TryGetValue("firebase", out var firebase) && 
            firebase is Dictionary<string, object> firebaseData &&
            firebaseData.TryGetValue("sign_in_provider", out var provider))
        {
            var providerStr = provider.ToString() ?? "";
            return providerStr switch
            {
                "google.com" => AuthProvider.GOOGLE,
                "apple.com" => AuthProvider.APPLE,
                "password" => AuthProvider.EMAIL,
                _ => providerStr.ToUpperInvariant()
            };
        }
        return "UNKNOWN";
    }
}
