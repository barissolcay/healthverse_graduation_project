using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthVerse.SharedKernel.Abstractions;

public interface ISystemCheckService
{
    Task<(bool IsHealthy, Dictionary<string, object> Details)> CheckDatabaseAsync();
}
