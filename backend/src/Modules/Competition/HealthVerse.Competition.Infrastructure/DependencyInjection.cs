using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Competition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCompetitionInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILeagueRoomRepository, LeagueRoomRepository>();
        services.AddScoped<ILeagueMemberRepository, LeagueMemberRepository>();
        services.AddScoped<ILeagueConfigRepository, LeagueConfigRepository>();
        services.AddScoped<IUserPointsHistoryRepository, UserPointsHistoryRepository>();
        services.AddScoped<ICompetitionUserRepository, CompetitionUserRepository>();
        services.AddScoped<ICompetitionUnitOfWork, CompetitionUnitOfWork>();

        return services;
    }
}
