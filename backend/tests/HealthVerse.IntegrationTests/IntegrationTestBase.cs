using System;
using System.Net.Http;
using System.Threading.Tasks;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Xunit;

namespace HealthVerse.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected HttpClient Client = default!;
    private IServiceScope _scope = default!;
    private Respawner? _respawner;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected HealthVerseDbContext DbContext => _scope.ServiceProvider.GetRequiredService<HealthVerseDbContext>();

    public virtual async Task InitializeAsync()
    {
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost")
        });

        // Note: Individual tests should set X-User-Id header with a valid Guid
        // Default header removed because 'test-user' is not a valid Guid

        _scope = Factory.Services.CreateScope();

        await ResetDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        // await DbContext.Database.EnsureDeletedAsync(); // Removed to preserve schema between tests
        _scope.Dispose();
        Client.Dispose();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(Factory.ConnectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public", "identity", "competition", "social", "gamification", "tasks", "missions", "notifications", "notification" },
            WithReseed = true
        });

        await _respawner.ResetAsync(connection);
    }
}
