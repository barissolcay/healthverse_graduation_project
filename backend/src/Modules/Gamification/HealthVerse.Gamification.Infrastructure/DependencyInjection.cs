using HealthVerse.Gamification.Application;
using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Gamification.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Gamification module infrastructure services (repositories, ports).
    /// </summary>
    public static IServiceCollection AddGamificationInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IPointTransactionRepository, PointTransactionRepository>();
        services.AddScoped<IUserDailyStatsRepository, UserDailyStatsRepository>();
        services.AddScoped<IMilestoneRepository, MilestoneRepository>();

        // Unit of Work
        services.AddScoped<IGamificationUnitOfWork, GamificationUnitOfWork>();

        services.AddGamificationApplication();

        return services;
    }
}
