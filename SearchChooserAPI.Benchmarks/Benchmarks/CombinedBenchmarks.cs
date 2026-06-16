using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

public class CombinedBenchmarks : BenchmarkBase
{
    [Benchmark(Description = "Search + filter (name + experience)")]
    public async Task Search_And_Filter()
    {
        var r = Req();
        r.Search = "John";
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "5"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Search + filter + column include")]
    public async Task Search_Filter_Include()
    {
        var r = Req();
        r.Search = "Smith";
        r.Columns = ["DoctorId", "DoctorName", "SpecialtyName", "YearsOfExperience"];
        r.Mode = ColumnMode.Include;
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "Rating",
                Operator = FilterOperator.Gt,
                Value = "3.0"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Multiple filters + exclude column")]
    public async Task MultiFilter_Exclude()
    {
        var r = Req();
        r.Columns = ["LastActive", "JoinDate"];
        r.Mode = ColumnMode.Exclude;
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "3"
            },
            new FilterCriteria
            {
                ColumnName = "Rating",
                Operator = FilterOperator.Gt,
                Value = "3.5"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Search across projected columns")]
    public async Task Search_ProjectedColumns()
    {
        var r = Req();
        r.Search = "Cardiology";
        r.Columns = ["DoctorId", "DoctorName", "SpecialtyName"];
        r.Mode = ColumnMode.Include;
        await RunQuery(r);
    }

    [Benchmark(Description = "Full pipeline: search + multiple filters + include")]
    public async Task FullPipeline()
    {
        var r = Req();
        r.Search = "a";
        r.Columns = ["DoctorId", "DoctorName", "SpecialtyName", "Degree", "YearsOfExperience", "Rating"];
        r.Mode = ColumnMode.Include;
        r.Filters =
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
        ];
        await RunQuery(r);
    }
}
