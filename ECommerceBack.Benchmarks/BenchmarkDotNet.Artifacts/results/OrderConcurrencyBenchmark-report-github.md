```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8328/25H2/2025Update/HudsonValley2)
13th Gen Intel Core i5-13420H 2.10GHz, 1 CPU, 12 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.12 (8.0.12, 8.0.1224.60305), X64 RyuJIT x86-64-v3
  Job-ZYKMEL : .NET 8.0.12 (8.0.12, 8.0.1224.60305), X64 RyuJIT x86-64-v3

Runtime=.NET 8.0  IterationCount=5  LaunchCount=1  
WarmupCount=2  

```
| Method                         | ConcurrentRequests | Mean       | Error      | StdDev      | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|------------------------------- |------------------- |-----------:|-----------:|------------:|------:|--------:|----------:|----------:|----------:|------------:|
| **CreateMultipleOrders_NoLimit**   | **5**                  |   **305.6 ms** |   **567.2 ms** |    **87.78 ms** |  **1.09** |    **0.49** |         **-** |         **-** |   **2.11 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 5                  |   474.0 ms |   329.0 ms |    85.43 ms |  1.70 |    0.68 |         - |         - |   2.34 MB |        1.11 |
|                                |                    |            |            |             |       |         |           |           |           |             |
| **CreateMultipleOrders_NoLimit**   | **10**                 | **1,675.7 ms** | **1,825.6 ms** |   **474.10 ms** |  **1.06** |    **0.38** | **1000.0000** |         **-** |   **8.03 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 10                 | 1,820.8 ms | 1,437.9 ms |   373.43 ms |  1.15 |    0.36 | 1000.0000 |         - |   8.04 MB |        1.00 |
|                                |                    |            |            |             |       |         |           |           |           |             |
| **CreateMultipleOrders_NoLimit**   | **30**                 | **6,859.7 ms** | **6,584.0 ms** | **1,018.88 ms** |  **1.02** |    **0.20** | **5000.0000** | **3000.0000** |     **34 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 30                 | 2,301.7 ms |   888.9 ms |   230.85 ms |  0.34 |    0.06 | 2000.0000 | 1000.0000 |  13.55 MB |        0.40 |
