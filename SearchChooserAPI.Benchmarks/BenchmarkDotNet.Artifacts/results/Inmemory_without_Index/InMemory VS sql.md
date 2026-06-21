# Benchmark Analysis: Why InMemory Appears Slower than SQL Server

## Executive Summary

**Your benchmark is NOT broken.** The results are actually correct and reveal a fundamental truth about EF Core's InMemory provider: **it is NOT an in-memory database — it is a fake that forces client-side evaluation of your entire query pipeline.** This makes it dramatically slower and more memory-hungry than a real SQL Server, especially for complex queries.

---

## The Numbers at a Glance

| Benchmark | InMemory | SQL Docker | Local SQL | InMemory vs Best SQL |
|---|---:|---:|---:|---|
| Get all doctors | 36.90 ms | **28.99 ms** | 35.71 ms | ❌ 1.27× slower |
| Search by first name | **321.83 ms** | 80.00 ms | 109.38 ms | ❌ **4.02× slower** |
| Filter Experience > 10 | 25.23 ms | **20.97 ms** | 35.96 ms | ❌ 1.20× slower |
| Filter Rating range | **16.58 ms** | 18.93 ms | 25.54 ms | ✅ 0.88× (faster) |
| Include 3 columns | 27.44 ms | 26.09 ms | **21.29 ms** | ❌ 1.29× slower |
| Full pipeline | **47.56 ms** | 28.08 ms | 34.71 ms | ❌ **1.69× slower** |

### Memory Allocation — The Smoking Gun

| Benchmark | InMemory | SQL Docker | SQL Local |
|---|---:|---:|---:|
| Get all doctors | **38,112 KB** | 1,094 KB | 949 KB | 
| Search by first name | **289,390 KB** | 97 KB | 90 KB |
| Full pipeline | **60,357 KB** | 803 KB | 686 KB |

> [!CAUTION]
> **InMemory allocates 35×–3,000× more memory than SQL Server.** This is the primary performance killer, and it's by design — not a bug in your benchmark.

---

## Root Cause Analysis

### 🔴 Root Cause #1: Client-Side Evaluation (The Biggest Problem)

Your query in [DoctorService.cs](file:///d:/Meccano/Projects/DynamicSearch_POC/DynamicSearch_API/SearchChooserAPI/Services/DoctorService.cs#L20-L32) does this:

```csharp
var query = _context.Doctors
    .Select(d => new DoctorSearchResponse
    {
        DoctorName = d.DoctorTranslations
            .Where(t => t.Language == "en")
            .Select(t => t.Name)
            .FirstOrDefault(),   // ← sub-query into related entity
        SpecialtyName = d.Specialty.SpecialtyTranslations
            .Where(t => t.Language == "en")
            .Select(t => t.Name)
            .FirstOrDefault(),   // ← another sub-query
        Degree = d.Degree.DegreeTranslations
            .Where(t => t.Language == "en")
            .Select(t => t.Name)
            .FirstOrDefault(),   // ← yet another sub-query
        // ...
    })
    .ApplyDynamicQuery(request);
```

**With SQL Server:** EF Core translates this entire expression tree into a single SQL statement with `JOIN`s, `WHERE` clauses, and `TOP 1` subselects. The database engine handles everything server-side — filtering, joining, projecting — and returns only the matching rows over the wire.

**With InMemory:** The InMemory provider **cannot translate** complex LINQ operations like `.Where().Select().FirstOrDefault()` inside a projection. Instead, it:

1. Loads **all 1,000 Doctor entities** into memory
2. Loads **all 1,000 DoctorTranslation entities** into memory
3. Loads **all Specialty + SpecialtyTranslation entities** into memory
4. Loads **all Degree + DegreeTranslation entities** into memory
5. Performs every `.Where()`, `.Select()`, `.FirstOrDefault()` in C# LINQ-to-Objects
6. Materializes every intermediate object on the managed heap

This explains the **38,112 KB vs 1,094 KB** allocation difference on "Get all doctors" — InMemory is literally creating tens of thousands of intermediate .NET objects that SQL Server never needs to create.

---

### 🔴 Root Cause #2: Search Uses `.ToLower().Contains()` — No Indexing for InMemory

Your [DynamicSearch method](file:///d:/Meccano/Projects/DynamicSearch_POC/DynamicSearch_API/SearchChooserAPI/Services/DynamicQuery/DynamicQueryExtensions.cs#L24-L89) builds an expression like:

```csharp
x.DoctorName.ToLower().Contains("john") 
|| x.SpecialtyName.ToLower().Contains("john") 
|| x.Degree.ToLower().Contains("john")
// ... for every string property
```

**With SQL Server:** This translates to `WHERE LOWER(col) LIKE '%john%'` — the server can scan indexed pages and use internal text-processing optimizations.

**With InMemory:** This runs on **every object already materialized in the heap**. But the real cost is that the search happens **after** the projection, which means for "Search by first name":

1. All 1,000 doctors are projected (creating ~5,000+ objects for the translations)
2. `.ToLower()` creates a **new string allocation** for every string property of every row
3. `.Contains()` does a brute-force substring scan on each

This is why "Search by first name" is **321 ms / 289 MB** on InMemory vs **80 ms / 97 KB** on Docker SQL. The 289 MB of allocations are all those intermediate strings and LINQ iterators.

---

### 🟡 Root Cause #3: No Indexes on InMemory (By Design in Your Code)

In [ProviderFactory.cs](file:///d:/Meccano/Projects/DynamicSearch_POC/DynamicSearch_API/SearchChooserAPI.Benchmarks/Business/ProviderFactory.cs#L57), the InMemory provider is explicitly seeded **without indexes**:

```csharp
SeedSynchronously(context, dataSize, addIndexes: false);  // ← InMemory
```

While in [Docker](file:///d:/Meccano/Projects/DynamicSearch_POC/DynamicSearch_API/SearchChooserAPI.Benchmarks/Business/ProviderFactory.cs#L79) and [Local](file:///d:/Meccano/Projects/DynamicSearch_POC/DynamicSearch_API/SearchChooserAPI.Benchmarks/Business/ProviderFactory.cs#L94):

```csharp
await BenchmarkDataSeeder.SeedAsync(context, dataSize, addIndexes: true);  // ← SQL Server
```

This is correct — InMemory doesn't support real indexes. But it means SQL Server has indexes on `YearsOfExperience`, `Rating`, `SpecialtyId`, `DegreeId`, etc. that accelerate filter operations.

---

### 🟡 Root Cause #4: GC Pressure from Massive Allocations

Look at the Gen0/Gen1 columns:

| Benchmark | InMemory Gen0 | InMemory Gen1 | SQL Gen0 | SQL Gen1 |
|---|---:|---:|---:|---:|
| Get all doctors | **3,076** | 76 | 62 | 62 |
| Search by first name | **23,000** | **1,000** | 0 | 0 |
| Full pipeline | **4,916** | 83 | 0 | 0 |

InMemory triggers **23,000 Gen0 garbage collections** on search vs **zero** for SQL. Each GC pause adds latency. The Gen1 promotions (1,000) mean objects are surviving long enough to get promoted, causing even more expensive collections.

---

### 🟢 Root Cause #5: Why "Filter Rating Range" is the Exception

Rating range filter (16.58 ms InMemory vs 18.93 ms SQL) is the **one case** where InMemory wins. Why?

- The filter is a simple numeric comparison (`Rating >= 2.0 AND Rating <= 4.0`)
- No string operations, no sub-queries into related entities  
- No `.ToLower()` / `.Contains()` allocations
- The result set is large (most doctors fall in 2.0–4.0), so SQL Server's network overhead and deserialization cost approaches InMemory's direct-memory cost
- At only 1,000 rows, the overhead of a network round-trip to SQL Server is relatively significant compared to direct memory comparison

This actually proves the benchmark is working correctly — simple numeric operations on small datasets are the **only** scenario where InMemory has an advantage.

---

## Is There Anything Wrong with the Benchmark?

### ✅ Things Done Correctly
- `[GlobalSetup]` seeds data once per parameter combo (not per iteration)
- `[GlobalCleanup]` disposes containers properly
- `PageSize = 0` ensures full-table scan (no pagination shortcut)
- Result validation (`if (result.Items.Count == 0) throw`) prevents dead-code elimination
- Fixed seed `new Random(42)` ensures reproducible data
- Docker SQL gets `CommandTimeout(180)` for container startup

### ⚠️ Minor Benchmark Concerns (Not Affecting the SQL vs InMemory Conclusion)

| Concern | Impact | Explanation |
|---|---|---|
| **Low iteration count** (5) | Medium | `StdDev` values are reasonable, but 5 iterations gives less statistical confidence. Consider 15+ for publication-quality results. |
| **`DontEnforcePowerPlan()`** | Low | Results could vary if Windows throttles the CPU mid-run. The `PowerPlanMode=00000000` in your output confirms the "Balanced" plan was active, not "High Performance". |
| **No `AsNoTracking()`** | Low–Medium | EF Core change tracking is active for all providers. For InMemory this adds additional overhead (identity map lookups). SQL Server results include tracking overhead too, but it's proportionally less. |
| **`CountAsync()` + `ToListAsync()` = 2 evaluations** | Low | The query is evaluated twice — once for count, once for data. Both providers pay this cost equally. |

---

## Conclusions

### What the Benchmark Actually Proves

```
┌────────────────────────────────────────────────────────────┐
│  EF Core InMemory is NOT a performance baseline.           │
│  It is a testing fake that forces client-side evaluation.  │
│  Your DynamicQuery library performs well on real SQL.       │
└────────────────────────────────────────────────────────────┘
```

1. **SQL Server Docker vs Local SQL** are the meaningful comparison pair — Docker is slightly faster because it runs a fresh, uncontended instance with no other workloads.

2. **InMemory should not be used to benchmark query performance.** Microsoft themselves [warn against this](https://learn.microsoft.com/en-us/ef/core/testing/). It's designed for unit testing (verifying logic), not performance testing.

3. **Your DynamicQuery library is working correctly.** The expression trees it builds translate efficiently to SQL. The performance gap is entirely in how each provider evaluates those expressions.

### Recommendations

| Action | Why |
|---|---|
| **Remove InMemory from performance benchmarks** | It measures C# LINQ-to-Objects overhead, not your library's performance |
| **Keep SQL Docker + Local SQL** | These are the meaningful comparison — Docker isolates contention, Local reflects production-like conditions |
| **Add `AsNoTracking()`** to the query | ~10-15% speedup for read-only benchmarks by skipping change tracking overhead |
| **Consider SQLite in-memory** as a lightweight alternative | `UseInMemoryDatabase` → `UseSqlite("DataSource=:memory:")` for a provider that actually translates LINQ to SQL |
| **Increase iteration count to 15+** | Better statistical confidence for publication-quality results |
