using System.Diagnostics;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Api.Application.Queries;

public class GetSystemStatusQuery : IRequest<SystemStatusResponse>
{
}

public class SystemStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Version { get; set; } = "1.0.0";
    public Dictionary<string, object> Checks { get; set; } = new();
}

public class GetSystemStatusQueryHandler : IRequestHandler<GetSystemStatusQuery, SystemStatusResponse>
{
    private readonly ISystemCheckService _systemCheckService;

    public GetSystemStatusQueryHandler(ISystemCheckService systemCheckService)
    {
        _systemCheckService = systemCheckService;
    }

    public async Task<SystemStatusResponse> Handle(GetSystemStatusQuery request, CancellationToken cancellationToken)
    {
        var checks = new Dictionary<string, object>();
        var isHealthy = true;

        // DB Check via Service (Infrastructure)
        var (dbHealthy, dbDetails) = await _systemCheckService.CheckDatabaseAsync();
        checks["database"] = dbDetails;
        if (!dbHealthy) isHealthy = false;

        // Memory check (Local logic)
        try 
        {
            var process = Process.GetCurrentProcess();
            checks["memory"] = new
            {
                workingSetMb = process.WorkingSet64 / 1024 / 1024,
                gcTotalMemoryMb = GC.GetTotalMemory(false) / 1024 / 1024
            };
        }
        catch 
        {
            // Ignore memory check fail
        }

        return new SystemStatusResponse
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Checks = checks
        };
    }
}
