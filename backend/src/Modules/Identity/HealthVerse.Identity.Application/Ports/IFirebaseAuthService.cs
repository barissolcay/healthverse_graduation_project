namespace HealthVerse.Identity.Application.Ports;

/// <summary>
/// Firebase authentication service port.
/// Abstracts Firebase SDK to allow testing and hexagonal architecture compliance.
/// </summary>
public interface IFirebaseAuthService
{
    /// <summary>
    /// Verify Firebase ID token and extract user info.
    /// </summary>
    /// <param name="idToken">Firebase ID token from client</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Firebase user info or null if invalid</returns>
    Task<FirebaseUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}

/// <summary>
/// Firebase user info extracted from token.
/// </summary>
public sealed record FirebaseUserInfo(
    string Uid,
    string Email,
    string Provider
);
