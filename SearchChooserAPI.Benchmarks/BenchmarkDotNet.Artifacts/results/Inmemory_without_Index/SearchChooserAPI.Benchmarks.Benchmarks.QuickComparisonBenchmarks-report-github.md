```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8655)
Unknown processor
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-NBZTAO : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

PowerPlanMode=00000000-0000-0000-0000-000000000000  IterationCount=5  MaxIterationCount=7  
MinIterationCount=3  WarmupCount=2  

```
| Method                            | Provider        | Mean      | Error     | StdDev   | Median    | Op/s   | Gen0       | Gen1      | Allocated    |
|---------------------------------- |---------------- |----------:|----------:|---------:|----------:|-------:|-----------:|----------:|-------------:|
| **&#39;Get all doctors&#39;**                 | **InMemory**        |  **36.90 ms** |  **2.442 ms** | **0.378 ms** |  **37.06 ms** | **27.103** |  **3076.9231** |   **76.9231** |  **38112.33 KB** |
| &#39;Search by first name&#39;            | InMemory        | 321.83 ms | 17.432 ms | 2.698 ms | 321.50 ms |  3.107 | 23000.0000 | 1000.0000 | 289390.95 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | InMemory        |  25.23 ms |  7.745 ms | 2.011 ms |  24.35 ms | 39.643 |  2250.0000 |  125.0000 |  27784.13 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | InMemory        |  16.58 ms |  4.794 ms | 0.742 ms |  16.93 ms | 60.302 |  1656.2500 |   31.2500 |  20438.42 KB |
| &#39;Include 3 columns&#39;               | InMemory        |  27.44 ms |  0.614 ms | 0.095 ms |  27.44 ms | 36.441 |  2906.2500 |   31.2500 |  35667.35 KB |
| &#39;Full: search + filter + include&#39; | InMemory        |  47.56 ms |  6.850 ms | 1.060 ms |  47.65 ms | 21.028 |  4916.6667 |   83.3333 |  60357.96 KB |
| **&#39;Get all doctors&#39;**                 | **SqlServerDocker** |  **28.99 ms** |  **3.155 ms** | **0.819 ms** |  **29.08 ms** | **34.489** |    **62.5000** |   **62.5000** |   **1094.74 KB** |
| &#39;Search by first name&#39;            | SqlServerDocker |  80.00 ms | 12.261 ms | 3.184 ms |  78.87 ms | 12.500 |          - |         - |     97.34 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | SqlServerDocker |  20.97 ms |  1.156 ms | 0.300 ms |  20.97 ms | 47.694 |    62.5000 |   62.5000 |    865.45 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | SqlServerDocker |  18.93 ms |  1.279 ms | 0.332 ms |  18.90 ms | 52.823 |    31.2500 |   31.2500 |    683.82 KB |
| &#39;Include 3 columns&#39;               | SqlServerDocker |  26.09 ms | 14.760 ms | 3.833 ms |  25.22 ms | 38.323 |    62.5000 |   62.5000 |    826.07 KB |
| &#39;Full: search + filter + include&#39; | SqlServerDocker |  28.08 ms |  6.065 ms | 1.575 ms |  28.23 ms | 35.614 |          - |         - |    803.46 KB |
| **&#39;Get all doctors&#39;**                 | **LocalSqlServer**  |  **35.71 ms** | **17.615 ms** | **4.575 ms** |  **32.95 ms** | **28.007** |    **76.9231** |         **-** |     **949.6 KB** |
| &#39;Search by first name&#39;            | LocalSqlServer  | 109.38 ms | 11.875 ms | 3.084 ms | 108.67 ms |  9.143 |          - |         - |     90.08 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | LocalSqlServer  |  35.96 ms |  7.040 ms | 1.089 ms |  36.32 ms | 27.810 |          - |         - |    719.59 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | LocalSqlServer  |  25.54 ms | 10.815 ms | 2.809 ms |  26.28 ms | 39.160 |    31.2500 |         - |    535.34 KB |
| &#39;Include 3 columns&#39;               | LocalSqlServer  |  21.29 ms |  1.969 ms | 0.511 ms |  21.08 ms | 46.964 |    62.5000 |   31.2500 |    767.78 KB |
| &#39;Full: search + filter + include&#39; | LocalSqlServer  |  34.71 ms |  2.545 ms | 0.661 ms |  34.85 ms | 28.811 |          - |         - |    686.86 KB |
