using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

public class FilteringBenchmarks : BenchmarkBase
{
    [Benchmark(Description = "Filter YearsOfExperience > 10")]
    public async Task Filter_Experience_Gt()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "10"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Filter Rating >= 4.0")]
    public async Task Filter_Rating_Gte()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "Rating",
                Operator = FilterOperator.Gt,
                Value = "3.9"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Filter YearsOfExperience < 5")]
    public async Task Filter_Experience_Lt()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Lt,
                Value = "5"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Filter YearsOfExperience = 0")]
    public async Task Filter_Experience_Eq()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Eq,
                Value = "0"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Filter YearsOfExperience != 0 (NotEqual)")]
    public async Task Filter_Experience_Neq()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Neq,
                Value = "0"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Filter Rating range 2.0-4.0")]
    public async Task Filter_Rating_Range()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "Rating",
                Operator = FilterOperator.Range,
                Value = "2.0",
                Value2 = "4.0"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "Filter by JoinDate year range")]
    public async Task Filter_JoinDate_Range()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "JoinDate",
                Operator = FilterOperator.Range,
                Value = "2010-01-01",
                Value2 = "2020-12-31"
            }
        ];
        await RunQuery(r);
    }
}
