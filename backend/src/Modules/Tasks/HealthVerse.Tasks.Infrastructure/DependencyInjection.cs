using HealthVerse.Contracts.Health;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Infrastructure.Persistence;
using HealthVerse.Tasks.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthVerse.Tasks.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Tasks module infrastructure services (repositories, ports, health updaters).
    /// </summary>
    public static IServiceCollection AddTasksInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserTaskRepository, UserTaskRepository>();
        services.AddScoped<ITaskTemplateRepository, TaskTemplateRepository>();
        services.AddScoped<IUserGoalRepository, UserGoalRepository>();
        services.AddScoped<IUserInterestRepository, UserInterestRepository>();

        // Unit of Work
        services.AddScoped<ITasksUnitOfWork, TasksUnitOfWork>();

        // Health Progress Updaters
        services.AddScoped<IHealthProgressUpdater, GoalsProgressUpdater>();
        services.AddScoped<IHealthProgressUpdater, TasksProgressUpdater>();

        return services;
    }
}
