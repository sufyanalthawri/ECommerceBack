```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8457/25H2/2025Update/HudsonValley2)
13th Gen Intel Core i5-13420H 2.10GHz, 1 CPU, 12 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.12 (8.0.12, 8.0.1224.60305), X64 RyuJIT x86-64-v3
  Job-ZYKMEL : .NET 8.0.12 (8.0.12, 8.0.1224.60305), X64 RyuJIT x86-64-v3

Runtime=.NET 8.0  IterationCount=5  LaunchCount=1  
WarmupCount=2  

```
| Method                         | ConcurrentRequests | Mean        | Error       | StdDev     | Median      | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|------------------------------- |------------------- |------------:|------------:|-----------:|------------:|------:|--------:|----------:|----------:|----------:|------------:|
| **CreateMultipleOrders_NoLimit**   | **5**                  |    **422.9 ms** |    **459.3 ms** |   **119.3 ms** |    **337.5 ms** |  **1.06** |    **0.37** |         **-** |         **-** |   **2.04 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 5                  |    423.8 ms |    460.1 ms |   119.5 ms |    337.2 ms |  1.06 |    0.37 |         - |         - |   2.04 MB |        1.00 |
|                                |                    |             |             |            |             |       |         |           |           |           |             |
| **CreateMultipleOrders_NoLimit**   | **10**                 |  **1,703.3 ms** |  **1,904.9 ms** |   **494.7 ms** |  **1,521.3 ms** |  **1.07** |    **0.40** |         **-** |         **-** |   **5.52 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 10                 |  1,756.8 ms |    831.5 ms |   215.9 ms |  1,898.5 ms |  1.10 |    0.31 | 1000.0000 |         - |   8.04 MB |        1.46 |
|                                |                    |             |             |            |             |       |         |           |           |           |             |
| **CreateMultipleOrders_NoLimit**   | **30**                 | **12,469.5 ms** | **15,974.5 ms** | **4,148.5 ms** | **10,975.1 ms** |  **1.09** |    **0.46** | **5000.0000** | **3000.0000** |  **29.49 MB** |        **1.00** |
| CreateMultipleOrders_WithLimit | 30                 |  2,614.8 ms |  1,165.7 ms |   302.7 ms |  2,405.2 ms |  0.23 |    0.07 | 2000.0000 | 1000.0000 |  13.44 MB |        0.46 |
