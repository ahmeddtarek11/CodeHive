using CodeHive.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace CodeHive.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;

    public WebApplicationFactory<Program> ApplicationFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("codehive_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgres.StartAsync();

        ApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
            });

        using var scope = ApplicationFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CodeHiveDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (ApplicationFactory is not null)
        {
            await ApplicationFactory.DisposeAsync();
        }

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }
}
