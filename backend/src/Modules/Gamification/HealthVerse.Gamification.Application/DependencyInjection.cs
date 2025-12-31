using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HealthVerse.Gamification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGamificationApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
