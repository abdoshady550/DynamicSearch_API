# Dynamic Search API — NBomber Load Tests

HTTP-level load testing project using NBomber to measure throughput, latency, and error rates against the running `SearchChooserAPI`.

## Prerequisites

The API must be running before starting load tests:

```powershell
cd SearchChooserAPI
dotnet run -c Release
```

Default API URL: `http://localhost:5157`.

## How to Run

### Quick Start (defaults)

```powershell
cd SearchChooserAPI.LoadTests
dotnet run -c Release
```

Defaults: `--url http://localhost:5157 --rate 50 --duration 30 --warmup 10`.

### Full Options

```powershell
dotnet run -c Release -- --url http://localhost:5157 --rate 100 --duration 60 --warmup 15
```

| Option | Short | Default | Description |
|---|---|---|---|
| `--url` | `-u` | `http://localhost:5157` | Target API base URL |
| `--rate` | `-r` | `50` | Requests per second |
| `--duration` | `-d` | `30` | Test duration in seconds |
| `--warmup` | `-w` | `10` | Warmup duration in seconds |
| `--scenario` | `-s` | *(all)* | Run a single scenario by name |

### Run a Specific Scenario

```powershell
dotnet run -c Release -- --scenario get_all_doctors
dotnet run -c Release -- --scenario mixed_workload --rate 200 --duration 120
```

### From Solution Root

```powershell
dotnet run -c Release --project SearchChooserAPI.LoadTests -- --url http://localhost:5157 --rate 100
```

## Load Scenarios

### `get_all_doctors`

Issues a `POST /api/Doctors` with an empty search body (`{ pageSize: 20, pageNumber: 1 }`). Measures baseline endpoint throughput with no query constraints.

### `search_by_name`

Issues `POST /api/Doctors` with `{ search: "John", pageSize: 20, pageNumber: 1 }`. Measures text-search across doctor name/specialty columns.

### `filter_by_experience`

Issues `POST /api/Doctors` with a filter: `YearsOfExperience > 10`. Measures simple numeric range filtering.

### `random_workload`

Picks a random request from the predefined payloads (`AllRequests`) per invocation. Simulates varied user behaviour.

### `mixed_workload`

Samples from all 6 predefined request types using `Random.Shared.Next()`. Best approximation of real-world traffic mix.

## Request Payloads (6 types)

| Payload | Description |
|---|---|
| `GetAllDoctorsRequest` | No search, no filters, no projections |
| `SearchByNameRequest` | Search = "John" |
| `FilterByExperienceRequest` | `YearsOfExperience > 10` |
| `IncludeColumnsRequest` | Include only `DoctorId, DoctorName, SpecialtyName` |
| `MultiFilterRequest` | `YearsOfExperience > 5 AND Rating > 3.5` |
| `FullPipelineRequest` | Search + filter + column include |

All use `PageSize = 20, PageNumber = 1`.

## Measured Metrics

| Metric | Description |
|---|---|
| **RPS** | Requests per second (throughput) |
| **Latency mean / p50 / p75 / p95 / p99** | Response time distribution |
| **Latency min / max** | Fastest / slowest request |
| **Latency std dev** | Variability |
| **OK count / Fail count** | Success vs error breakdown |
| **Data transfer** | Min / mean / max / total bytes per scenario |
| **Error rate %** | `(total_fail / total_requests) × 100` |

## Output Reports

Reports are saved to `SearchChooserAPI.LoadTests/Reports/` as:

| Format | File Pattern | Content |
|---|---|---|
| HTML | `load_test_report_*.html` | Interactive charts and tables |
| CSV | `load_test_report_*.csv` | Raw measurement data |
| Markdown | `load_test_report_*.md` | Summary tables |

The console also prints a full results summary with per-scenario breakdown.

## Exit Codes

| Code | Condition |
|---|---|
| `0` | All scenarios: fail count ≤ 10% of ok count |
| `1` | Any scenario: fail count > 10% of ok count |

## Architecture

```
SearchChooserAPI.LoadTests/
├── Program.cs                       Entry: CLI parsing, health check, NBomberRunner, summary
├── Scenarios/
│   └── DoctorScenarios.cs           5 scenario factories (Create*Scenario)
├── Data/
│   └── LoadTestData.cs              6 predefined DoctorSearchRequest payloads + helpers
└── SearchChooserAPI.LoadTests.csproj  net10.0, NBomber 5.1, NBomber.Http 5.1
```

### Key Design Decisions

| Decision | Rationale |
|---|---|
| **NBomber over custom scripts** | Built-in RPS injection, latency histograms, reporting, warmup phase |
| **Separate HttpClient** | Shared across scenarios; 30s timeout for slow queries at 100K rows |
| **Health check before run** | Early feedback if target API is unreachable |
| **`[MemoryDiagnoser]` not in load tests** | Memory profiling happens in BenchmarkDotNet; load tests focus on throughput/latency |
| **6 predefined payloads + random** | Covers common query patterns while allowing targeted scenario runs |
| **Reports dir in project root** | Survives `dotnet clean`; easy to find after CI runs |

## Interpreting Results

**Key indicators of performance issues:**

| Signal | What it means |
|---|---|
| **P95 >> mean** | Long-tail requests — likely GC pauses or query plan recompilation |
| **Error rate > 1%** | Timeouts, server errors (5xx), or request failures under load |
| **RPS plateaus while rate increases** | Server saturated — reached max throughput |
| **Data transfer varies wildly** | Column projection not working as expected; full entities returned |
| **Latency increases over test duration** | Memory leak, connection pool exhaustion, or tempdb growth |
