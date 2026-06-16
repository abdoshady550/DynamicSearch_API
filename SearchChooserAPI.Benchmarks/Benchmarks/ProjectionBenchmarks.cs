using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

public class ProjectionBenchmarks : BenchmarkBase
{
    [Benchmark(Description = "Include mode: 3 columns only")]
    public async Task Include_ThreeColumns()
    {
        var r = Req();
        r.Columns = ["DoctorId", "DoctorName", "SpecialtyName"];
        r.Mode = ColumnMode.Include;
        await RunQuery(r);
    }

    [Benchmark(Description = "Include mode: all columns explicit")]
    public async Task Include_AllColumns()
    {
        var r = Req();
        r.Columns = ["DoctorId", "DoctorName", "SpecialtyName", "Degree", "YearsOfExperience", "Rating", "JoinDate", "LastActive"];
        r.Mode = ColumnMode.Include;
        await RunQuery(r);
    }

    [Benchmark(Description = "Exclude mode: remove 2 columns")]
    public async Task Exclude_TwoColumns()
    {
        var r = Req();
        r.Columns = ["LastActive", "JoinDate"];
        r.Mode = ColumnMode.Exclude;
        await RunQuery(r);
    }

    [Benchmark(Description = "Exclude mode: remove 5 columns (minimal)")]
    public async Task Exclude_Minimal()
    {
        var r = Req();
        r.Columns = ["DoctorName", "SpecialtyName", "Degree", "Rating", "JoinDate"];
        r.Mode = ColumnMode.Exclude;
        await RunQuery(r);
    }

    [Benchmark(Description = "Default projection (all columns)")]
    public async Task Default_Projection()
    {
        await RunQuery(Req());
    }
}
