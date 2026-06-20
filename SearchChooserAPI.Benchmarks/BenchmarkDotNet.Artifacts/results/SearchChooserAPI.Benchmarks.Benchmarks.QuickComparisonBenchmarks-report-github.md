```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8655)
Unknown processor
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-EMSOCX : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

PowerPlanMode=00000000-0000-0000-0000-000000000000  IterationCount=5  MaxIterationCount=7  
MinIterationCount=3  WarmupCount=2  

```
| Method                            | Provider        | Mean      | Error     | StdDev    | Median     | Op/s   | Gen0       | Gen1     | Allocated    |
|---------------------------------- |---------------- |----------:|----------:|----------:|-----------:|-------:|-----------:|---------:|-------------:|
| **&#39;Get all doctors&#39;**                 | **InMemory**        |  **34.05 ms** |  **4.082 ms** |  **1.060 ms** |  **33.436 ms** | **29.372** |  **3076.9231** |  **76.9231** |  **38111.06 KB** |
| &#39;Search by first name&#39;            | InMemory        | 266.59 ms | 83.543 ms | 21.696 ms | 271.814 ms |  3.751 | 23333.3333 | 333.3333 | 289389.88 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | InMemory        |  26.79 ms |  3.466 ms |  0.536 ms |  26.979 ms | 37.327 |  2250.0000 | 125.0000 |  27783.71 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | InMemory        |  19.89 ms |  3.835 ms |  0.996 ms |  20.480 ms | 50.266 |  1656.2500 |  31.2500 |  20437.96 KB |
| &#39;Include 3 columns&#39;               | InMemory        |  33.68 ms | 15.365 ms |  3.990 ms |  33.498 ms | 29.687 |  2875.0000 |  62.5000 |  35669.46 KB |
| &#39;Full: search + filter + include&#39; | InMemory        |  50.93 ms | 12.026 ms |  1.861 ms |  51.178 ms | 19.634 |  4800.0000 | 100.0000 |  58944.22 KB |
| **&#39;Get all doctors&#39;**                 | **SqlServerDocker** |  **21.23 ms** |  **4.981 ms** |  **0.771 ms** |  **21.477 ms** | **47.113** |    **62.5000** |  **62.5000** |   **1094.59 KB** |
| &#39;Search by first name&#39;            | SqlServerDocker |  48.03 ms |  5.926 ms |  1.539 ms |  48.024 ms | 20.822 |          - |        - |     98.22 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | SqlServerDocker |  16.58 ms |  0.840 ms |  0.130 ms |  16.616 ms | 60.315 |    62.5000 |  62.5000 |    865.37 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | SqlServerDocker |  13.36 ms |  2.703 ms |  0.418 ms |  13.551 ms | 74.851 |    46.8750 |  46.8750 |    683.29 KB |
| &#39;Include 3 columns&#39;               | SqlServerDocker |  15.18 ms |  1.577 ms |  0.410 ms |  15.188 ms | 65.887 |    62.5000 |  62.5000 |    826.48 KB |
| &#39;Full: search + filter + include&#39; | SqlServerDocker |  21.27 ms |  0.541 ms |  0.141 ms |  21.253 ms | 47.013 |    62.5000 |  62.5000 |    801.85 KB |
| **&#39;Get all doctors&#39;**                 | **LocalSqlServer**  |  **18.18 ms** |  **3.623 ms** |  **0.941 ms** |  **18.476 ms** | **55.001** |    **62.5000** |  **31.2500** |    **948.53 KB** |
| &#39;Search by first name&#39;            | LocalSqlServer  |  54.68 ms | 16.174 ms |  4.200 ms |  52.622 ms | 18.289 |          - |        - |     88.83 KB |
| &#39;Filter YearsOfExperience &gt; 10&#39;   | LocalSqlServer  |  13.01 ms |  3.566 ms |  0.926 ms |  12.903 ms | 76.855 |    46.8750 |        - |    717.27 KB |
| &#39;Filter Rating range 2.0-4.0&#39;     | LocalSqlServer  |  10.29 ms |  2.160 ms |  0.561 ms |  10.303 ms | 97.200 |    31.2500 |        - |    534.67 KB |
| &#39;Include 3 columns&#39;               | LocalSqlServer  |  10.35 ms |  4.815 ms |  1.250 ms |   9.769 ms | 96.613 |    62.5000 |  15.6250 |    767.76 KB |
| &#39;Full: search + filter + include&#39; | LocalSqlServer  |  18.78 ms |  4.079 ms |  0.631 ms |  18.881 ms | 53.258 |    31.2500 |        - |    688.55 KB |
