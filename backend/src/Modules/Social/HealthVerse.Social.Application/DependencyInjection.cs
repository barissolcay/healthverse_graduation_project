using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HealthVerse.Social.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSocialApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
