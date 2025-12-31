using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HealthVerse.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HealthVerseDbContext>
{
    public HealthVerseDbContext CreateDbContext(string[] args)
    {
        // Find the Api project's appsettings.json
        var currentDir = Directory.GetCurrentDirectory();
        var basePath = currentDir;
        
        // Navigate to find appsettings.json
        if (Directory.Exists(Path.Combine(currentDir, "Api", "HealthVerse.Api")))
        {
            basePath = Path.Combine(currentDir, "Api", "HealthVerse.Api");
        }
        else if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            basePath = currentDir;
        }
        
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HealthVerseDbContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=healthverse;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("HealthVerse.Infrastructure"));

        return new HealthVerseDbContext(optionsBuilder.Options);
    }
}
