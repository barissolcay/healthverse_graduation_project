using HealthVerse.Contracts.Health;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Infrastructure.Persistence;
using HealthVerse.Missions.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Missions.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Missions module infrastructure services (repositories, ports).
    /// Note: INotificationService is registered in Notifications.Infrastructure.
    /// </summary>
    public static IServiceCollection AddMissionsInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IPartnerMissionRepository, PartnerMissionRepository>();
        services.AddScoped<IPartnerMissionSlotRepository, PartnerMissionSlotRepository>();
        services.AddScoped<IGlobalMissionRepository, GlobalMissionRepository>();
        services.AddScoped<IGlobalMissionParticipantRepository, GlobalMissionParticipantRepository>();
        services.AddScoped<IGlobalMissionContributionRepository, GlobalMissionContributionRepository>();

        // Unit of Work
        services.AddScoped<IMissionsUnitOfWork, MissionsUnitOfWork>();

        // Shared Services (cross-module adapters)
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<IMissionsUserService, MissionsUserService>();

        // Health Progress Updaters
        services.AddScoped<IHealthProgressUpdater, PartnerMissionsProgressUpdater>();
        services.AddScoped<IHealthProgressUpdater, GlobalMissionsProgressUpdater>();

        return services;
    }
}
