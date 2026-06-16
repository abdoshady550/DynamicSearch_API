using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;
using SearchChooserAPI.Models.Req;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

public class SearchBenchmarks : BenchmarkBase
{
    [Benchmark(Description = "Get all doctors (no search)")]
    public async Task Get_All_Doctors()
    {
        await RunQuery(Req());
    }

    [Benchmark(Description = "Search by common first name")]
    public async Task Search_By_FirstName()
    {
        var r = Req();
        r.Search = "John";
        await RunQuery(r);
    }

    [Benchmark(Description = "Search by common last name")]
    public async Task Search_By_LastName()
    {
        var r = Req();
        r.Search = "Smith";
        await RunQuery(r);
    }

    [Benchmark(Description = "Search by specialty name")]
    public async Task Search_By_Specialty()
    {
        var r = Req();
        r.Search = "Cardiology";
        await RunQuery(r);
    }

    [Benchmark(Description = "Search by partial name match")]
    public async Task Search_Partial_Match()
    {
        var r = Req();
        r.Search = "Jo";
        await RunQuery(r);
    }

    [Benchmark(Description = "Search across all columns (numeric)")]
    public async Task Search_Numeric()
    {
        var r = Req();
        r.Search = "10";
        await RunQuery(r);
    }

    [Benchmark(Description = "Search with no matching results")]
    public async Task Search_NoResults()
    {
        var r = Req();
        r.Search = "ZzZzZzZzNonExistent";
        await RunQueryAllowEmpty(r);
    }
}
