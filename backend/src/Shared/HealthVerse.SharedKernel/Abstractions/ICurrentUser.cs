namespace HealthVerse.SharedKernel.Abstractions;

/// <summary>
/// Mevcut kullanıcı (Current User) bilgilerine erişim sağlayan arayüz.
/// Hexagonal mimaride "Application" katmanının "Infrastructure"dan (HttpContext) izole olmasını sağlar.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Mevcut kullanıcının GUID kimliği.
    /// Kullanıcı giriş yapmamışsa exception fırlatır veya Guid.Empty döner (tasarıma göre değişir).
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Kullanıcının kimliğinin doğrulanıp doğrulanmadığı.
    /// </summary>
    bool IsAuthenticated { get; }
}
