using HealthVerse.Contracts.Health;
using HealthVerse.Social.Application;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Infrastructure.Persistence;
using HealthVerse.Social.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Social.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Social module infrastructure services (repositories, ports).
    /// </summary>
    public static IServiceCollection AddSocialInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IUserBlockRepository, UserBlockRepository>();
        services.AddScoped<IDuelRepository, DuelRepository>();
        services.AddScoped<ISocialUserRepository, SocialUserRepository>();

        // Unit of Work
        services.AddScoped<ISocialUnitOfWork, SocialUnitOfWork>();

        // Health Progress Updaters
        services.AddScoped<IHealthProgressUpdater, DuelsProgressUpdater>();

        // Application Layer
        services.AddSocialApplication();

        return services;
    }
}
