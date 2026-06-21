```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8655)
Unknown processor
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-ROHQEX : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

PowerPlanMode=00000000-0000-0000-0000-000000000000  IterationCount=5  MaxIterationCount=7  
MinIterationCount=3  WarmupCount=2  

```
| Method                            | Provider        | Mean      | Error     | StdDev   | Median    | Op/s   | Gen0       | Gen1     | Allocated    |
|---------------------------------- |---------------- |----------:|----------:|---------:|----------:|-------:|-----------:|---------:|-------------:|
| **&#39;Get all doctors&#39;**                 | **InMemory**        |  **27.34 ms** |  **4.897 ms** | **0.758 ms** |  **27.55 ms** | **36.577** |  **3093.7500** |  **31.2500** |  **38109.76 KB** |
| &#39;Search by first name&#39;            | InMemory        | 229.11 ms | 31.561 ms | 8.196 ms | 228.53 ms |  4.365 | 23333.3333 | 333.3333 | 289390.71 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | InMemory        |  19.30 ms |  2.612 ms | 0.404 ms |  19.42 ms | 51.808 |  2250.0000 | 125.0000 |  27783.38 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | InMemory        |  17.39 ms | 10.264 ms | 2.666 ms |  16.67 ms | 57.489 |  1656.2500 |  15.6250 |  20438.72 KB |
| &#39;Include 3 columns&#39;               | InMemory        |  24.82 ms |  3.136 ms | 0.814 ms |  25.24 ms | 40.291 |  2906.2500 |  31.2500 |   35667.8 KB |
| &#39;Full: search + filter + include&#39; | InMemory        |  48.18 ms | 14.400 ms | 2.228 ms |  47.91 ms | 20.756 |  4416.6667 |  83.3333 |  54914.62 KB |
| **&#39;Get all doctors&#39;**                 | **SqlServerDocker** |  **20.90 ms** |  **5.494 ms** | **1.427 ms** |  **20.48 ms** | **47.839** |    **62.5000** |  **62.5000** |   **1095.92 KB** |
| &#39;Search by first name&#39;            | SqlServerDocker |  49.62 ms |  3.458 ms | 0.898 ms |  49.46 ms | 20.152 |          - |        - |     96.93 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | SqlServerDocker |  14.80 ms |  1.017 ms | 0.157 ms |  14.82 ms | 67.571 |    62.5000 |  62.5000 |     865.4 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | SqlServerDocker |  12.20 ms |  4.487 ms | 1.165 ms |  11.81 ms | 81.938 |    46.8750 |  46.8750 |    682.95 KB |
| &#39;Include 3 columns&#39;               | SqlServerDocker |  13.69 ms |  5.800 ms | 1.506 ms |  12.75 ms | 73.024 |    62.5000 |  62.5000 |    825.91 KB |
| &#39;Full: search + filter + include&#39; | SqlServerDocker |  17.74 ms |  0.480 ms | 0.074 ms |  17.75 ms | 56.364 |    62.5000 |  62.5000 |       804 KB |
| **&#39;Get all doctors&#39;**                 | **LocalSqlServer**  |  **17.05 ms** |  **8.438 ms** | **2.191 ms** |  **15.77 ms** | **58.655** |    **62.5000** |  **15.6250** |    **947.79 KB** |
| &#39;Search by first name&#39;            | LocalSqlServer  |  42.51 ms | 10.135 ms | 1.568 ms |  42.72 ms | 23.524 |          - |        - |        88 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | LocalSqlServer  |  12.97 ms |  4.293 ms | 1.115 ms |  13.07 ms | 77.108 |    46.8750 |        - |    717.78 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | LocalSqlServer  |  10.05 ms |  1.890 ms | 0.491 ms |  10.10 ms | 99.513 |    31.2500 |        - |    535.54 KB |
| &#39;Include 3 columns&#39;               | LocalSqlServer  |  12.62 ms |  7.958 ms | 2.067 ms |  11.89 ms | 79.265 |    62.5000 |  15.6250 |    767.56 KB |
| &#39;Full: search + filter + include&#39; | LocalSqlServer  |  21.73 ms |  9.835 ms | 2.554 ms |  20.33 ms | 46.019 |    31.2500 |        - |    687.24 KB |
