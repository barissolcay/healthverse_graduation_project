using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Infrastructure.Services;

public class SystemCheckService : ISystemCheckService
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly ILogger<SystemCheckService> _logger;

    public SystemCheckService(HealthVerseDbContext dbContext, ILogger<SystemCheckService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(bool IsHealthy, Dictionary<string, object> Details)> CheckDatabaseAsync()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            return (canConnect, new Dictionary<string, object>
            {
                { "status", canConnect ? "Healthy" : "Unhealthy" },
                { "provider", "PostgreSQL" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return (false, new Dictionary<string, object>
            {
                { "status", "Unhealthy" },
                { "error", ex.Message }
            });
        }
    }
}
