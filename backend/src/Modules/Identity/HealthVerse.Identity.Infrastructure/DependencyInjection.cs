using HealthVerse.Identity.Application.Ports;
using HealthVerse.Identity.Infrastructure.Auth;
using HealthVerse.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Identity.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Identity module infrastructure services (repositories, ports).
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<Application.Ports.IUserRepository, UserRepository>();
        services.AddScoped<Domain.Ports.IUserRepository, UserRepository>();
        services.AddScoped<IAuthIdentityRepository, AuthIdentityRepository>();

        // Unit of Work
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Ports / Adapters
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

        return services;
    }
}
