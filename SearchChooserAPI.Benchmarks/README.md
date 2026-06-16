# Dynamic Search API — Performance Benchmarks

BenchmarkDotNet suite comparing search/filter/projection performance across three database providers at four dataset sizes.

## Provider Comparison

| Provider | Description | Setup Cost | Real SQL? | Indexes? | Use Case |
|---|---|---|---|---|---|
| **InMemory** | EF Core InMemory Provider | None | No | No | Dev iterations, CI fast-feedback |
| **SqlServerDocker** | Testcontainers MsSql container | ~5s start + seed | Yes | Yes | CI/CD, isolated perf testing |
| **LocalSqlServer** | Local SQL Server instance | Seed only | Yes | Yes | Production-mirror perf validation |

## Dataset Sizes

Controlled via `[ParamsSource]`: **100**, **1,000**, **10,000**, **100,000** doctors.

Each doctor has a randomized name (100 first × 100 last names), specialty (12 types), degree (5 types), experience (0–40 yr), rating (1.0–5.0), join/last-active dates, and an English translation.

For SQL Server providers, indexes are created on: `YearsOfExperience`, `Rating`, `SpecialtyId`, `DegreeId`, `DoctorTranslations.DoctorId`, `DoctorTranslations.Language`.

## Quick Comparison (Fast Mode)

**`QuickComparisonBenchmarks`** — runs in ~2 minutes. Fixed at 1,000 rows, all 3 providers, 6 representative benchmarks, 2 warmup + 5 iterations.

```powershell
dotnet run -c Release -- --filter *Quick*
```

| Benchmark | Description |
|---|---|
| `Get_All_Doctors` | No search/filter/projection |
| `Search_By_FirstName` | `search: "John"` |
| `Filter_Experience_Gt` | `YearsOfExperience > 10` |
| `Filter_Rating_Range` | `Rating 2.0–4.0` |
| `Include_ThreeColumns` | Include `DoctorId, DoctorName, SpecialtyName` |
| `FullPipeline` | Search + 2 filters + column include |

### Expected Time: ~2 minutes (3 providers × 6 benchmarks)

| Provider | Setup | Benchmarks | Total |
|---|---|---|---|
| InMemory | ~0s | ~10s | ~10s |
| Docker SQL | ~5s | ~20s | ~25s |
| Local SQL | ~2s | ~20s | ~22s |

## Benchmark Categories (Full Suite)

### `SearchBenchmarks` — 7 benchmarks

| Method | Description | Search Value |
|---|---|---|
| `Get_All_Doctors` | No search criteria; returns all rows | — |
| `Search_By_FirstName` | Exact-ish match on "John" | `John` |
| `Search_By_LastName` | Exact-ish match on "Smith" | `Smith` |
| `Search_By_Specialty` | Match on specialty name | `Cardiology` |
| `Search_Partial_Match` | Two-character prefix | `Jo` |
| `Search_Numeric` | Numeric search scalar | `10` |
| `Search_NoResults` | Non-existent term (all-fail) | `ZzZzZzZzNonExistent` |

### `FilteringBenchmarks` — 7 benchmarks

| Method | Column | Operator | Value(s) |
|---|---|---|---|
| `Filter_Experience_Gt` | `YearsOfExperience` | `Gt` | `10` |
| `Filter_Rating_Gte` | `Rating` | `Gt` | `3.9` |
| `Filter_Experience_Lt` | `YearsOfExperience` | `Lt` | `5` |
| `Filter_Experience_Eq` | `YearsOfExperience` | `Eq` | `0` |
| `Filter_Experience_Neq` | `YearsOfExperience` | `Neq` | `0` |
| `Filter_Rating_Range` | `Rating` | `Range` | `2.0` – `4.0` |
| `Filter_JoinDate_Range` | `JoinDate` | `Range` | `2010-01-01` – `2020-12-31` |

### `ProjectionBenchmarks` — 5 benchmarks

| Method | Mode | Columns |
|---|---|---|
| `Include_ThreeColumns` | `Include` | `DoctorId, DoctorName, SpecialtyName` |
| `Include_AllColumns` | `Include` | All 8 columns |
| `Exclude_TwoColumns` | `Exclude` | `LastActive, JoinDate` |
| `Exclude_Minimal` | `Exclude` | 5 columns removed (3 kept) |
| `Default_Projection` | — | No column spec (full entity) |

### `CombinedBenchmarks` — 5 benchmarks

| Method | Search | Filters | Columns |
|---|---|---|---|
| `Search_And_Filter` | `John` | `YearsOfExperience > 5` | — |
| `Search_Filter_Include` | `Smith` | `Rating > 3.0` | `Include: 4 columns` |
| `MultiFilter_Exclude` | — | `Exp > 3 AND Rating > 3.5` | `Exclude: 2 columns` |
| `Search_ProjectedColumns` | `Cardiology` | — | `Include: 3 columns` |
| `FullPipeline` | `a` | `Exp > 2 AND Rating 2.0–5.0` | `Include: 6 columns` |

## Measured Metrics

| Metric | Source | Description |
|---|---|---|
| **Mean** | BenchmarkDotNet | Average execution time |
| **Median** | BenchmarkDotNet | 50th percentile |
| **Min / Max** | BenchmarkDotNet | Fastest / slowest iteration |
| **Op/s** | BenchmarkDotNet | Operations per second |
| **Gen 0/1/2** | `[MemoryDiagnoser]` | GC collections per 1000 ops |
| **Allocated** | `[MemoryDiagnoser]` | Bytes allocated per op |

## Output Artifacts

After a run, results appear under `SearchChooserAPI.Benchmarks\bin\Release\net10.0\*\`:

| File | Format | Content |
|---|---|---|
| `*-report.csv` | CSV | Raw measurement data |
| `*-report-github.md` | Markdown | GitHub-flavored table |
| `*-report.html` | HTML | Interactive report |
| `*-measurements.csv` | CSV | Detailed per-iteration data |
| `*-barplot.png` | PNG | Bar chart (if R installed) |

## How to Run

### Run All Benchmarks (Interactive Menu)

```powershell
cd SearchChooserAPI.Benchmarks
dotnet run -c Release
```

Shows a menu: 0 = all, 1 = Search, 2 = Filtering, 3 = Projection, 4 = Combined.

### Filter by Category, Provider, or Size

```powershell
# Only SearchBenchmarks
dotnet run -c Release -- --filter *Search*

# Only InMemory (all categories)
dotnet run -c Release -- --filter *InMemory*

# Only 100-row datasets
dotnet run -c Release -- --filter *100*

# Only InMemory Search benchmarks at 100 rows
dotnet run -c Release -- --filter "*Search*InMemory*100*"
```

### Run a Specific Benchmark Class Directly

```powershell
dotnet run -c Release -- --job dry   # quick dry-run to verify setup
```

### Run from Solution Root

```powershell
dotnet run -c Release --project SearchChooserAPI.Benchmarks
```

## Architecture

```
SearchChooserAPI.Benchmarks/
├── Program.cs                           Entry point + BenchmarkSwitcher
├── Benchmarks/
│   ├── BenchmarkBase.cs                 Abstract: [Params], GlobalSetup/Cleanup, helpers
│   ├── SearchBenchmarks.cs              7 search scenarios
│   ├── FilteringBenchmarks.cs           7 filter scenarios
│   ├── ProjectionBenchmarks.cs          5 projection scenarios
│   └── CombinedBenchmarks.cs            5 combined scenarios
├── Business/
│   ├── BenchmarkProvider.cs             Enum: InMemory / SqlServerDocker / LocalSqlServer
│   ├── ProviderFactory.cs               Creates DbContext + DoctorService per provider
│   └── BenchmarkDataSeeder.cs           Generates N doctors with realistic data + indexes
├── Configs/
│   └── BenchmarkConfigs.cs              ManualConfig: warmup, iterations, diagnosers, exporters
└── SearchChooserAPI.Benchmarks.csproj   net10.0, BenchmarkDotNet 0.14, Testcontainers.MsSql 4.12
```

### Key Design Decisions

| Decision | Rationale |
|---|---|
| **Single base class + 4 derived** | Avoids 12 separate files; `[Params]` handles provider×size combinatorics |
| **`[MemoryDiagnoser]`** | Measures GC allocations critical for high-throughput API scenarios |
| **PageSize = 0** | Benchmarks measure full materialization, not paginated subsets |
| **Stateless Docker container per run** | Fresh container each GlobalSetup; disposed in GlobalCleanup — no cross-contamination |
| **Batch seeding (1000/batch) + ChangeTracker.Clear** | Prevents EF memory blowup at 100K rows |
| **Indexes only for SQL providers** | Reflects real production behaviour; InMemory ignores indexes anyway |
| **5 warmup + 5–15 iterations** | Balances measurement stability vs total run time |

## Expected Run Times

| Provider | DataSize | Per-Benchmark | Category (×7) | All 4 Categories |
|---|---|---|---|---|
| InMemory | 100 | ~1 ms | ~2 s | ~30 s |
| InMemory | 100K | ~100 ms | ~4 s | ~2 min |
| Docker | 100 | ~3 s (incl. startup+seed) | ~8 s | ~4 min |
| Docker | 100K | ~15 s (incl. seed) | ~30 s | ~8 min |
| **All** | **All** | **~48 combinations** | **—** | **~30–60 min** |

> **Note**: Docker startup + seed dominates time at small data sizes. At 100K, seed time (batch inserts) dominates.

## Interpreting Results

**Fastest execution ≠ best metric.** Consider:

- **InMemory** is unrealistically fast — no SQL translation, no I/O, no locking, no indexes.
- **Docker SQL** runs on a container (likely slower I/O than native) but tests real SQL generation and indexing.
- **Local SQL** is the closest to production — use it as your baseline.

**Key questions to answer:**

1. **How does mean/median latency scale from 100 → 100K rows?** (linear? exponential?)
2. **Do P95 latencies diverge from mean?** (indicates GC pauses or query plan shifts)
3. **Which projection mode (Include vs Exclude) performs better for your typical column set?**
4. **Do combined benchmarks amplify individual costs or overlap?**
5. **Which provider has the best Op/s at each data size?**
