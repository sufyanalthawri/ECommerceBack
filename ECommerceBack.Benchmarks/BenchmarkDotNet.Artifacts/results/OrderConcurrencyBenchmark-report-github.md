```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8457/25H2/2025Update/HudsonValley2)
13th Gen Intel Core i5-13420H 2.10GHz, 1 CPU, 12 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.12 (8.0.12, 8.0.1224.60305), X64 RyuJIT x86-64-v3
  Job-ZYKMEL : .NET 8.0.12 (8.0.12, 8.0.1224.60305), X64 RyuJIT x86-64-v3

Runtime=.NET 8.0  IterationCount=5  LaunchCount=1  
WarmupCount=2  

```
| Method                         | ConcurrentRequests | Mean        | Error        | StdDev      | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|------------------------------- |------------------- |------------:|-------------:|------------:|------:|--------:|----------:|----------:|----------:|------------:|
| **CreateMultipleOrders_NoLimit**   | **5**                  |    **339.0 ms** |     **44.90 ms** |     **6.95 ms** |  **1.00** |    **0.03** |         **-** |         **-** |   **2.04 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 5                  |    480.4 ms |    423.42 ms |   109.96 ms |  1.42 |    0.30 |         - |         - |   2.72 MB |        1.33 |
|                                |                    |             |              |             |       |         |           |           |           |             |
| **CreateMultipleOrders_NoLimit**   | **10**                 |  **1,769.3 ms** |  **2,158.81 ms** |   **560.64 ms** |  **1.08** |    **0.45** | **1000.0000** |         **-** |    **6.7 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 10                 |  2,024.4 ms |  2,103.04 ms |   546.15 ms |  1.24 |    0.47 | 1000.0000 |         - |   7.53 MB |        1.12 |
|                                |                    |             |              |             |       |         |           |           |           |             |
| **CreateMultipleOrders_NoLimit**   | **30**                 | **11,999.6 ms** | **17,233.73 ms** | **4,475.55 ms** |  **1.12** |    **0.56** | **9000.0000** | **6000.0000** |  **52.17 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 30                 |  2,372.9 ms |     37.28 ms |     5.77 ms |  0.22 |    0.08 | 2000.0000 | 1000.0000 |  13.43 MB |        0.26 |
