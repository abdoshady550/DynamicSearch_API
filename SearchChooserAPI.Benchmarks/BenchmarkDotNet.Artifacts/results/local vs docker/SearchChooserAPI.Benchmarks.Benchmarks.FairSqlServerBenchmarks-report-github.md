```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8655)
Unknown processor
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-VGBJBW : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

PowerPlanMode=00000000-0000-0000-0000-000000000000  IterationCount=5  MaxIterationCount=7  
MinIterationCount=3  WarmupCount=2  

```
| Method                            | Provider        | Mean      | Error     | StdDev    | Median    | Op/s   | Gen0    | Gen1    | Allocated  |
|---------------------------------- |---------------- |----------:|----------:|----------:|----------:|-------:|--------:|--------:|-----------:|
| **&#39;Get all doctors&#39;**                 | **SqlServerDocker** | **21.878 ms** |  **5.375 ms** | **0.8318 ms** | **21.724 ms** |  **45.71** | **62.5000** | **62.5000** | **1094.59 KB** |
| &#39;Search by first name&#39;            | SqlServerDocker | 43.880 ms | 10.266 ms | 1.5887 ms | 43.761 ms |  22.79 |       - |       - |   97.88 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | SqlServerDocker | 14.085 ms |  2.235 ms | 0.5804 ms | 14.012 ms |  71.00 | 62.5000 | 62.5000 |  865.45 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | SqlServerDocker | 12.374 ms |  6.693 ms | 1.7381 ms | 12.085 ms |  80.82 | 46.8750 | 46.8750 |  683.97 KB |
| &#39;Include 3 columns&#39;               | SqlServerDocker | 13.557 ms |  3.376 ms | 0.8767 ms | 13.090 ms |  73.76 | 62.5000 | 62.5000 |  825.63 KB |
| &#39;Full: search + filter + include&#39; | SqlServerDocker | 18.654 ms |  5.106 ms | 1.3259 ms | 18.382 ms |  53.61 | 62.5000 | 62.5000 |  803.46 KB |
| **&#39;Get all doctors&#39;**                 | **LocalSqlServer**  | **15.263 ms** |  **8.149 ms** | **2.1162 ms** | **14.442 ms** |  **65.52** | **62.5000** | **31.2500** |  **947.97 KB** |
| &#39;Search by first name&#39;            | LocalSqlServer  | 49.502 ms | 43.759 ms | 6.7717 ms | 50.152 ms |  20.20 |       - |       - |   89.36 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | LocalSqlServer  | 13.029 ms |  3.561 ms | 0.9248 ms | 13.299 ms |  76.75 | 46.8750 |       - |  718.26 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | LocalSqlServer  |  9.201 ms |  2.465 ms | 0.3814 ms |  9.276 ms | 108.69 | 31.2500 |       - |  534.73 KB |
| &#39;Include 3 columns&#39;               | LocalSqlServer  |  9.563 ms |  2.104 ms | 0.5463 ms |  9.717 ms | 104.57 | 62.5000 | 15.6250 |  768.17 KB |
| &#39;Full: search + filter + include&#39; | LocalSqlServer  | 18.636 ms |  4.735 ms | 0.7327 ms | 18.725 ms |  53.66 | 31.2500 |       - |   686.7 KB |
