using Microsoft.EntityFrameworkCore;
using SearchChooserAPI.Data;
using SearchChooserAPI.Services;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Configurations;

namespace SearchChooserAPI.Benchmarks.Business;

public sealed class BenchmarkContext : IAsyncDisposable
{
    private readonly IAsyncDisposable? _containerDisposable;

    public BenchmarkContext(SearchDbContext dbContext, IDoctorService service, IAsyncDisposable? container = null)
    {
        DbContext = dbContext;
        Service = service;
        _containerDisposable = container;
    }

    public SearchDbContext DbContext { get; }
    public IDoctorService Service { get; }

    public async ValueTask DisposeAsync()
    {
        if (_containerDisposable is not null)
            await _containerDisposable.DisposeAsync();

        await DbContext.DisposeAsync();
    }
}

public static class ProviderFactory
{
    private static readonly string LocalConnectionStringTemplate =
        Environment.GetEnvironmentVariable("BENCHMARK_LOCAL_CONNECTION")
        ?? "Server=.;Database=BenchmarkDocs_{0};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public static async Task<BenchmarkContext> CreateAsync(BenchmarkProvider provider, int dataSize)
    {
        return provider switch
        {
            BenchmarkProvider.InMemory => CreateInMemory(dataSize),
            BenchmarkProvider.SqlServerDocker => await CreateDockerAsync(dataSize),
            BenchmarkProvider.LocalSqlServer => await CreateLocalAsync(dataSize),
            _ => throw new ArgumentOutOfRangeException(nameof(provider))
        };
    }

    private static BenchmarkContext CreateInMemory(int dataSize)
    {
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseInMemoryDatabase($"Benchmark_{dataSize}_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging(false)
            .Options;

        var context = new SearchDbContext(options);
        SeedSynchronously(context, dataSize, addIndexes: false);

        return new BenchmarkContext(context, new DoctorService(context));
    }

    private static async Task<BenchmarkContext> CreateDockerAsync(int dataSize)
    {
        var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Bench@Pass123!")
            .WithCleanUp(true)
            .Build();

        await container.StartAsync();

        var connectionString = container.GetConnectionString();
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseSqlServer(connectionString, sql => sql
                .CommandTimeout(180)
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .Options;

        var context = new SearchDbContext(options);
        await BenchmarkDataSeeder.SeedAsync(context, dataSize, addIndexes: true);

        return new BenchmarkContext(context, new DoctorService(context), container);
    }

    private static async Task<BenchmarkContext> CreateLocalAsync(int dataSize)
    {
        var connectionString = string.Format(LocalConnectionStringTemplate, dataSize);
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseSqlServer(connectionString, sql => sql
                .CommandTimeout(180)
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .Options;

        var context = new SearchDbContext(options);
        await BenchmarkDataSeeder.SeedAsync(context, dataSize, addIndexes: true);

        return new BenchmarkContext(context, new DoctorService(context));
    }

    private static void SeedSynchronously(SearchDbContext context, int dataSize, bool addIndexes)
    {
        BenchmarkDataSeeder.SeedAsync(context, dataSize, addIndexes)
            .GetAwaiter().GetResult();
    }

    public static string Describe(BenchmarkProvider provider) => provider switch
    {
        BenchmarkProvider.InMemory => "EF Core InMemory",
        BenchmarkProvider.SqlServerDocker => "SQL Server (Docker Testcontainer)",
        BenchmarkProvider.LocalSqlServer => "SQL Server (Local Instance)",
        _ => provider.ToString()
    };
}
