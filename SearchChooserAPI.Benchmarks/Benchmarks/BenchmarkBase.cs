using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;
using SearchChooserAPI.Benchmarks.Business;
using SearchChooserAPI.Benchmarks.Configs;
using SearchChooserAPI.Models.Req;
using SearchChooserAPI.Models.Res;
using SearchChooserAPI.Services;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RPlotExporter, CsvExporter, HtmlExporter, MarkdownExporterAttribute.GitHub]
[Config(typeof(BenchmarkConfig))]
public abstract class BenchmarkBase
{
    protected IDoctorService _service = null!;
    protected BenchmarkContext _context = null!;

    [Params(BenchmarkProvider.InMemory, BenchmarkProvider.SqlServerDocker, BenchmarkProvider.LocalSqlServer)]
    public BenchmarkProvider Provider { get; set; }

    [ParamsSource(nameof(DataSizes))]
    public int DataSize { get; set; }

    public static IEnumerable<int> DataSizes() => [100, 1000, 10000, 100000];

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

    protected DoctorSearchRequest Req() => new() { PageSize = 0 };

    protected async Task RunQuery(DoctorSearchRequest request)
    {
        var result = await _service.SearchDoctorsAsync(request);
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    protected async Task RunQueryAllowEmpty(DoctorSearchRequest request)
    {
        await _service.SearchDoctorsAsync(request);
    }
}
