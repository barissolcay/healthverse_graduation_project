using System;
using System.Linq;
using System.Threading.Tasks;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HealthVerseDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Integration");

        builder.ConfigureServices(services =>
        {
            // Replace database with Npgsql
            services.RemoveAll(typeof(DbContextOptions<HealthVerseDbContext>));
            services.RemoveAll(typeof(HealthVerseDbContext));
            services.RemoveAll(typeof(DbContextOptions));

            services.AddDbContext<HealthVerseDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString(), b => 
                    b.MigrationsAssembly("HealthVerse.Infrastructure"));
            });

            // Register ICurrentUser for tests (normally registered in ConfigureFirebase which is skipped)
            services.AddHttpContextAccessor();
            services.AddScoped<HealthVerse.SharedKernel.Abstractions.ICurrentUser, HealthVerse.Infrastructure.Auth.CurrentUserAdapter>();

            // Configure test authentication to bypass Firebase
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, options => { });
        });

        // Configure test services to replace Firebase auth middleware
        builder.ConfigureTestServices(services =>
        {
            // Remove any Firebase-related services
            services.RemoveAll(typeof(FirebaseAdmin.FirebaseApp));
        });
    }
}
