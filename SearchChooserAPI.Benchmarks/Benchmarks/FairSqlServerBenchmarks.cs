using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;
using SearchChooserAPI.Benchmarks.Business;
using SearchChooserAPI.Benchmarks.Configs;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Services;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

[Config(typeof(QuickBenchmarkConfig))]
public class FairSqlServerBenchmarks
{
    private IDoctorService _service = null!;
    private BenchmarkContext _context = null!;

    [Params(BenchmarkProvider.SqlServerDocker, BenchmarkProvider.LocalSqlServer,BenchmarkProvider.InMemory)]
    public BenchmarkProvider Provider { get; set; }

    private const int DataSize = 1000;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _context = await ProviderFactory.CreateAsync(Provider, DataSize);
        _service = _context.Service;
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _context.DisposeAsync();
    }

    [Benchmark(Description = "Get all doctors")]
    public async Task Get_All_Doctors()
    {
        var result = await _service.SearchDoctorsAsync(new DoctorSearchRequest { PageSize = 0 });
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Search by first name")]
    public async Task Search_By_FirstName()
    {
        var result = await _service.SearchDoctorsAsync(new DoctorSearchRequest
        {
            PageSize = 0,
            Search = "John"
        });
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Filter YearsOfExperience > 10")]
    public async Task Filter_Experience_Gt()
    {
        var result = await _service.SearchDoctorsAsync(new DoctorSearchRequest
        {
            PageSize = 0,
            Filters =
            [
                new FilterCriteria
                {
                    ColumnName = "YearsOfExperience",
                    Operator = FilterOperator.Gt,
                    Value = "10"
                }
            ]
        });
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Filter Rating range 2.0-4.0")]
    public async Task Filter_Rating_Range()
    {
        var result = await _service.SearchDoctorsAsync(new DoctorSearchRequest
        {
            PageSize = 0,
            Filters =
            [
                new FilterCriteria
                {
                    ColumnName = "Rating",
                    Operator = FilterOperator.Range,
                    Value = "2.0",
                    Value2 = "4.0"
                }
            ]
        });
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Include 3 columns")]
    public async Task Include_ThreeColumns()
    {
        var result = await _service.SearchDoctorsAsync(new DoctorSearchRequest
        {
            PageSize = 0,
            Columns = ["DoctorId", "DoctorName", "SpecialtyName"],
            Mode = ColumnMode.Include
        });
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Full: search + filter + include")]
    public async Task FullPipeline()
    {
        var result = await _service.SearchDoctorsAsync(new DoctorSearchRequest
        {
            PageSize = 0,
            Search = "a",
            Columns = ["DoctorId", "DoctorName", "SpecialtyName", "Degree", "YearsOfExperience", "Rating"],
            Mode = ColumnMode.Include,
            Filters =
            [
                new FilterCriteria
                {
                    ColumnName = "YearsOfExperience",
                    Operator = FilterOperator.Gt,
                    Value = "2"
                },
                new FilterCriteria
                {
                    ColumnName = "Rating",
                    Operator = FilterOperator.Range,
                    Value = "2.0",
                    Value2 = "5.0"
                }
            ]
        });
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }
}
