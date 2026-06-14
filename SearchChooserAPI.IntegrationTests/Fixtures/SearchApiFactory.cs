using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using SearchChooserAPI.Data;
using Testcontainers.MsSql;

namespace SearchChooserAPI.IntegrationTests.Fixtures;

public class SearchApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
#pragma warning disable CS0618
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test@Pass123!")
        .WithCleanUp(true)
        .Build();
#pragma warning restore CS0618

    private string _connectionString = null!;
    private Respawner _respawner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SearchDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<SearchDbContext>(options =>
                options.UseSqlServer(_connectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        DataSeeder.SeedData(context);

        await InitializeRespawner();
    }

    private async Task InitializeRespawner()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        DataSeeder.SeedData(context);
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
