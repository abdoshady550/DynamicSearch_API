using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;

namespace SearchChooserAPI.Benchmarks.Configs;

public class QuickBenchmarkConfig : ManualConfig
{
    public QuickBenchmarkConfig()
    {
        AddJob(Job.Default
            .WithWarmupCount(2)
            .WithIterationCount(5)
            .WithMinIterationCount(3)
            .WithMaxIterationCount(7)
            .DontEnforcePowerPlan());

        AddDiagnoser(MemoryDiagnoser.Default);

        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Median);
        AddColumn(StatisticColumn.OperationsPerSecond);

        AddExporter(CsvExporter.Default);
        AddExporter(MarkdownExporter.GitHub);
    }
}
