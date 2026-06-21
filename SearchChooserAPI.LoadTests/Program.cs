using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Contracts.Stats;
using SearchChooserAPI.LoadTests.Scenarios;

const string DefaultUrl = "http://localhost:5157";
const int DefaultRate = 50;
const int DefaultDurationSec = 30;
const int DefaultWarmupSec = 10;

Console.WriteLine("==============================================================");
Console.WriteLine("  Dynamic Search API - NBomber Load Testing");
Console.WriteLine("==============================================================");
Console.WriteLine();

var argsList = new List<string>(args);

var baseUrl = GetArg(argsList, "--url") ?? GetArg(argsList, "-u") ?? DefaultUrl;
var rate = int.Parse(GetArg(argsList, "--rate") ?? GetArg(argsList, "-r") ?? DefaultRate.ToString());
var durationSec = int.Parse(GetArg(argsList, "--duration") ?? GetArg(argsList, "-d") ?? DefaultDurationSec.ToString());
var warmupSec = int.Parse(GetArg(argsList, "--warmup") ?? GetArg(argsList, "-w") ?? DefaultWarmupSec.ToString());

if (argsList.Contains("--help") || argsList.Contains("-h"))
{
    Console.WriteLine("Usage: dotnet run -- [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --url, -u <url>       Target API URL (default: http://localhost:5157)");
    Console.WriteLine("  --rate, -r <n>        Requests per second (default: 50)");
    Console.WriteLine("  --duration, -d <s>    Test duration in seconds (default: 30)");
    Console.WriteLine("  --warmup, -w <s>      Warmup duration in seconds (default: 10)");
    Console.WriteLine("  --scenario, -s <name> Specific scenario to run (default: all)");
    Console.WriteLine("  --help, -h            Show this help");
    Console.WriteLine();
    Console.WriteLine("Scenarios: get_all_doctors, search_by_name, filter_by_experience, random_workload, mixed_workload,");
    Console.WriteLine("          complex_a, complex_b, complex_c,");
    Console.WriteLine("          odata_get_all_doctors, odata_search_by_name, odata_filter_by_experience,");
    Console.WriteLine("          odata_complex_a, odata_complex_b, odata_complex_c,");
    Console.WriteLine("          odata_random_workload, odata_mixed_workload");
    return 0;
}

var scenarioName = GetArg(argsList, "--scenario") ?? GetArg(argsList, "-s");

Console.WriteLine($"Target:      {baseUrl}");
Console.WriteLine($"Rate:        {rate} req/s");
Console.WriteLine($"Duration:    {durationSec}s");
Console.WriteLine($"Warmup:      {warmupSec}s");
if (scenarioName is not null)
    Console.WriteLine($"Scenario:    {scenarioName}");
Console.WriteLine();

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

// Quick health check
try
{
    var healthResp = await httpClient.GetAsync($"{baseUrl}/api/Doctors");
    Console.WriteLine($"Health check: {(healthResp.IsSuccessStatusCode ? "OK" : $"Failed ({healthResp.StatusCode})")}");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Warning: Cannot reach {baseUrl}. Make sure the API is running.");
    Console.WriteLine($"Error: {ex.Message}");
    Console.ResetColor();
    Console.WriteLine();
}

Console.WriteLine("Starting NBomber load test...");
Console.WriteLine();

var scenarios = new List<ScenarioProps>();

if (scenarioName is null || scenarioName == "get_all_doctors")
    scenarios.Add(DoctorScenarios.CreateGetAllDoctorsScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "search_by_name")
    scenarios.Add(DoctorScenarios.CreateSearchByNameScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "filter_by_experience")
    scenarios.Add(DoctorScenarios.CreateFilterByExperienceScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "random_workload")
    scenarios.Add(DoctorScenarios.CreateRandomWorkloadScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "mixed_workload")
    scenarios.Add(DoctorScenarios.CreateMixedScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "complex_a")
    scenarios.Add(DoctorScenarios.CreateComplexAScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "complex_b")
    scenarios.Add(DoctorScenarios.CreateComplexBScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "complex_c")
    scenarios.Add(DoctorScenarios.CreateComplexCScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_get_all_doctors")
    scenarios.Add(DoctorScenarios.CreateODataGetAllDoctorsScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_search_by_name")
    scenarios.Add(DoctorScenarios.CreateODataSearchByNameScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_filter_by_experience")
    scenarios.Add(DoctorScenarios.CreateODataFilterByExperienceScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_complex_a")
    scenarios.Add(DoctorScenarios.CreateODataComplexAScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_complex_b")
    scenarios.Add(DoctorScenarios.CreateODataComplexBScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_complex_c")
    scenarios.Add(DoctorScenarios.CreateODataComplexCScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_random_workload")
    scenarios.Add(DoctorScenarios.CreateODataRandomWorkloadScenario(httpClient, baseUrl));

if (scenarioName is null || scenarioName == "odata_mixed_workload")
    scenarios.Add(DoctorScenarios.CreateODataMixedScenario(httpClient, baseUrl));

if (scenarios.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Unknown scenario: {scenarioName}");
    Console.ResetColor();
    return 1;
}

var scenariosWithLoad = scenarios.Select(s => s
    .WithWarmUpDuration(TimeSpan.FromSeconds(warmupSec))
    .WithLoadSimulations(
        Simulation.Inject(rate: rate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(durationSec))
    )).ToArray();

var reportDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Reports");
Directory.CreateDirectory(reportDir);

var result = NBomberRunner
    .RegisterScenarios(scenariosWithLoad)
    .WithReportFileName($"load_test_report_{DateTime.Now:yyyyMMdd_HHmmss}")
    .WithReportFolder(reportDir)
    .WithReportFormats(ReportFormat.Html, ReportFormat.Csv, ReportFormat.Md)
    .Run();

Console.WriteLine();
Console.WriteLine($"Reports saved to: {reportDir}");

Console.WriteLine();
Console.WriteLine("==============================================================");
Console.WriteLine("  RESULTS SUMMARY");
Console.WriteLine("==============================================================");
Console.WriteLine($"  Total requests:       {result.AllRequestCount}");
Console.WriteLine($"  Total OK:             {result.AllOkCount}");
Console.WriteLine($"  Total failed:         {result.AllFailCount}");
Console.WriteLine($"  Total bytes:          {result.AllBytes:N0}");
Console.WriteLine($"  Error rate:           {(result.AllRequestCount > 0 ? (double)result.AllFailCount / result.AllRequestCount * 100 : 0):F2}%");
Console.WriteLine();

foreach (var scnStats in result.ScenarioStats)
{
    Console.WriteLine($"--- {scnStats.ScenarioName} ---");
    Console.WriteLine($"  OK count:            {scnStats.Ok.Request.Count}");
    Console.WriteLine($"  Fail count:          {scnStats.Fail.Request.Count}");
    Console.WriteLine($"  RPS:                 {scnStats.Ok.Request.RPS:F1}");
    Console.WriteLine($"  Latency mean:        {scnStats.Ok.Latency.MeanMs:F1} ms");
    Console.WriteLine($"  Latency p50:         {scnStats.Ok.Latency.Percent50:F1} ms");
    Console.WriteLine($"  Latency p75:         {scnStats.Ok.Latency.Percent75:F1} ms");
    Console.WriteLine($"  Latency p95:         {scnStats.Ok.Latency.Percent95:F1} ms");
    Console.WriteLine($"  Latency p99:         {scnStats.Ok.Latency.Percent99:F1} ms");
    Console.WriteLine($"  Latency min:         {scnStats.Ok.Latency.MinMs:F1} ms");
    Console.WriteLine($"  Latency max:         {scnStats.Ok.Latency.MaxMs:F1} ms");
    Console.WriteLine($"  Latency std dev:     {scnStats.Ok.Latency.StdDev:F1} ms");
    Console.WriteLine($"  Data transfer min:   {scnStats.Ok.DataTransfer.MinBytes} B");
    Console.WriteLine($"  Data transfer mean:  {scnStats.Ok.DataTransfer.MeanBytes} B");
    Console.WriteLine($"  Data transfer max:   {scnStats.Ok.DataTransfer.MaxBytes} B");
    Console.WriteLine($"  Data transfer all:   {scnStats.Ok.DataTransfer.AllBytes} B");
    Console.WriteLine();
}

var hasHighErrorRate = result.ScenarioStats.Any(s => s.AllFailCount > s.AllOkCount * 0.1);
return hasHighErrorRate ? 1 : 0;

static string? GetArg(List<string> args, string key)
{
    var idx = args.IndexOf(key);
    if (idx < 0 || idx >= args.Count - 1) return null;
    var value = args[idx + 1];
    args.RemoveRange(idx, 2);
    return value;
}
