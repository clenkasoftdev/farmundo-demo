using Microsoft.EntityFrameworkCore;
using Farmundo.Demo.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Farmundo.Demo.Tests.Database;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    public string ConnectionString => _pgContainer.GetConnectionString();

    public DatabaseFixture()
    {
        _pgContainer = new PostgreSqlBuilder()
            .WithDatabase("farmundo_demo_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithImage("postgres:16")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _pgContainer.StartAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
    }
}
