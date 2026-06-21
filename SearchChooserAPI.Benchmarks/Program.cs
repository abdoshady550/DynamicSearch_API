using BenchmarkDotNet.Running;
using SearchChooserAPI.Benchmarks.Benchmarks;

var switcher = new BenchmarkSwitcher(
[
    typeof(SearchBenchmarks),
    typeof(FilteringBenchmarks),
    typeof(ProjectionBenchmarks),
    typeof(CombinedBenchmarks),
    typeof(QuickComparisonBenchmarks),
    typeof(FairSqlServerBenchmarks),
    typeof(DoctorsControllerVsODataBenchmarks)
]);

Console.WriteLine("==============================================================");
Console.WriteLine("  Dynamic Search API - Performance Benchmark Suite");
Console.WriteLine("==============================================================");
Console.WriteLine();
Console.WriteLine("Available benchmark categories:");
Console.WriteLine("  0: ALL benchmarks (runs everything)");
Console.WriteLine("  1: SearchBenchmarks");
Console.WriteLine("  2: FilteringBenchmarks");
Console.WriteLine("  3: ProjectionBenchmarks");
Console.WriteLine("  4: CombinedBenchmarks");
Console.WriteLine("  5: QuickComparisonBenchmarks  (fast! ~2 min, 3 providers @ 1000 rows)");
Console.WriteLine("  6: FairSqlServerBenchmarks    (fair comparison! Local vs Docker SQL Server)");
Console.WriteLine("  7: DoctorsControllerVsODataBenchmarks    (Dynamic vs OData comparison)");
Console.WriteLine();
Console.WriteLine("Providers (selectable per type via --filter):");
Console.WriteLine("  - EF Core InMemory");
Console.WriteLine("  - SQL Server (Docker via Testcontainers)");
Console.WriteLine("  - SQL Server (Local Instance)");
Console.WriteLine();
Console.WriteLine("Data sizes: 100, 1,000, 10,000, 100,000 rows");
Console.WriteLine("  QuickComparison: fixed at 1,000 rows");
Console.WriteLine();
Console.WriteLine("Usage:");
Console.WriteLine("  dotnet run -c Release          (interactive mode)");
Console.WriteLine("  dotnet run -c Release -- --filter *Quick*");
Console.WriteLine("  dotnet run -c Release -- --filter *Search*");
Console.WriteLine("  dotnet run -c Release -- --filter *InMemory*");
Console.WriteLine("  dotnet run -c Release -- --filter *100*");
Console.WriteLine("==============================================================");
Console.WriteLine();

try
{
    var summary = switcher.Run(args);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Console.ResetColor();
    return 1;
}

return 0;
