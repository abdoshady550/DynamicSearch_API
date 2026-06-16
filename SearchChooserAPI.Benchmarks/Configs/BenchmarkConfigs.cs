using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;

namespace SearchChooserAPI.Benchmarks.Configs;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddJob(Job.Default
            .WithWarmupCount(5)
            .WithIterationCount(10)
            .WithMinIterationCount(5)
            .WithMaxIterationCount(15)
            .DontEnforcePowerPlan());

        AddDiagnoser(MemoryDiagnoser.Default);

        AddColumn(StatisticColumn.Min);
        AddColumn(StatisticColumn.Max);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Median);
        AddColumn(StatisticColumn.OperationsPerSecond);

        AddExporter(CsvExporter.Default);
        AddExporter(CsvMeasurementsExporter.Default);
        AddExporter(HtmlExporter.Default);
        AddExporter(MarkdownExporter.GitHub);
    }
}
