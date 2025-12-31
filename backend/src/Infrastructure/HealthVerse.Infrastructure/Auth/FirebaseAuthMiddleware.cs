using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HealthVerse.Infrastructure.Auth;

/// <summary>
/// Firebase JWT doğrulama ve Kimlik Eşleştirme (Mapping) middleware'i.
/// 1. Bearer Token -> Firebase UID
/// 2. Firebase UID -> DB Lookup (AuthIdentities) -> UserId (Guid)
/// 3. ClaimsPrincipal oluştur (user_id claim = Guid)
/// </summary>
public class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FirebaseAuthMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public FirebaseAuthMiddleware(RequestDelegate next, ILogger<FirebaseAuthMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, HealthVerseDbContext dbContext)
    {
        // Auth gerektirmeyen endpoint'ler
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        // Authorization header kontrolü
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            // Development bypass: X-User-Id header (Sadece Dev/Test ortamında)
            if (_env.IsDevelopment() && context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                if (Guid.TryParse(userIdHeader, out var devUserId))
                {
                    var devClaims = new List<Claim>
                    {
                        new Claim("user_id", devUserId.ToString()), // Bizim internal Guid
                        new Claim("firebase_uid", "dev-bypass"),
                        new Claim(ClaimTypes.Name, "Dev User")
                    };
                    var devIdentity = new ClaimsIdentity(devClaims, "DevBypass");
                    context.User = new ClaimsPrincipal(devIdentity);
                    await _next(context);
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Authorization header eksik." });
            return;
        }

        var token = authHeader.ToString();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(7).Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Token eksik." });
            return;
        }

        try
        {
            // 1. Firebase token doğrulama
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            var firebaseUid = decodedToken.Uid;

            // 2. DB Lookup: FirebaseUid -> UserId
            // Not: Performance için buraya output caching veya redis eklenebilir.
            // Şimdilik doğrudan DB sorgusu (indexli kolonda hızlıdır).
            var authIdentity = await dbContext.AuthIdentities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid);

            if (authIdentity == null)
            {
                // Kullanıcı Firebase'de var ama bizim DB'de yok -> Kayıt gerekli
                // Ancak bazı durumlarda (Register flow) token geçerli ama user yok.
                // Bu durumda 'user_id' claim'i olmadan devam edebilir mi?
                // Hayır, Auth gerektiren endpoint'ler UserId bekler.
                // 403 Forbidden dönmek daha doğru olabilir veya client'a "Register ol" sinyali.
                // Şimdilik 401: User not found in system.
                _logger.LogWarning("Firebase UID {Uid} için sistemde kullanıcı bulunamadı.", firebaseUid);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized; // Veya 403
                await context.Response.WriteAsJsonAsync(new { error = "Kullanıcı kaydı bulunamadı. Lütfen kayıt olun.", code = "USER_NOT_FOUND" });
                return;
            }

            // 3. Claims oluştur
            var claims = new List<Claim>
            {
                new Claim("user_id", authIdentity.UserId.ToString()), // KRİTİK: Bizim Guid UserId
                new Claim("firebase_uid", firebaseUid),
                new Claim(ClaimTypes.Email, decodedToken.Claims.GetValueOrDefault("email")?.ToString() ?? ""),
                new Claim("email_verified", decodedToken.Claims.GetValueOrDefault("email_verified")?.ToString() ?? "false")
            };

            // Provider bilgisi
            if (decodedToken.Claims.TryGetValue("firebase", out var firebase) && firebase is Dictionary<string, object> firebaseData)
            {
                if (firebaseData.TryGetValue("sign_in_provider", out var provider))
                {
                    claims.Add(new Claim("auth_provider", provider.ToString() ?? ""));
                }
            }

            var identity = new ClaimsIdentity(claims, "Firebase");
            context.User = new ClaimsPrincipal(identity);

            // X-Firebase-Uid header (gerekirse loglama için kalsın)
            context.Request.Headers["X-Firebase-Uid"] = firebaseUid;

            await _next(context);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Firebase token doğrulama hatası");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Geçersiz veya süresi dolmuş token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token doğrulama sırasında beklenmeyen hata");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Token doğrulama hatası." });
        }
    }

    private static bool IsPublicEndpoint(string path)
    {
        return path == "/" ||
               path.StartsWith("/swagger") ||
               path.StartsWith("/health") ||
               path == "/status" ||
               path.StartsWith("/status/live") ||
               path.StartsWith("/status/ready") ||
               path.StartsWith("/api/auth/register") ||
               path.StartsWith("/api/auth/login") ||
               path.StartsWith("/api/auth/dev-register") || // Dev registration izin ver
               path.StartsWith("/api/auth/dev-login");
    }

}

/// <summary>
/// Firebase yapılandırma extension.
/// </summary>
public static class FirebaseAuthExtensions
{
    public static void ConfigureFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        // ICurrentUser servisini burada kaydediyoruz (Auth paketiyle beraber)
        services.AddScoped<ICurrentUser, CurrentUserAdapter>();

        // Skip if already initialized
        if (FirebaseApp.DefaultInstance is not null)
            return;

        try
        {
            // Öncelik 1: User Secrets
            var credentialsJson = configuration["Firebase:CredentialsJson"];
            if (!string.IsNullOrEmpty(credentialsJson))
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(credentialsJson)
                });
                return;
            }

            // Öncelik 2: Dosya
            var credentialPath = configuration["Firebase:CredentialPath"];
            if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialPath)
                });
                return;
            }

            // Öncelik 3: Env Var
            var envCredentials = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS");
            if (!string.IsNullOrEmpty(envCredentials))
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(envCredentials)
                });
                return;
            }
        }
        catch (ArgumentException)
        {
            // Ignore if already initialized
        }
    }

    public static IApplicationBuilder UseFirebaseAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FirebaseAuthMiddleware>();
    }
}
