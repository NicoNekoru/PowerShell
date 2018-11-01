``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.10240.17146 (1507/RTM/Threshold1)
Intel Core i3-4150 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=3410090 Hz, Resolution=293.2474 ns, Timer=TSC
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|                                  Method |              a |             b |      Mean |      Error |      StdDev |    Median | Scaled | ScaledSD |
|---------------------------------------- |--------------- |-------------- |----------:|-----------:|------------:|----------:|-------:|---------:|
|                    **CoreFXCompareOrdinal** |   **CaseFolding1** |   **CaseFolding** |  **16.79 ns** |  **0.3706 ns** |   **0.4412 ns** |  **16.75 ns** |   **0.22** |     **0.01** |
| CoreFXCompareInvariantCultureIgnoreCase |   CaseFolding1 |   CaseFolding |  74.76 ns |  1.5277 ns |   2.6756 ns |  74.37 ns |   1.00 |     0.00 |
|                        ToLowerInvariant |   CaseFolding1 |   CaseFolding |  80.12 ns |  1.6634 ns |   3.4721 ns |  79.05 ns |   1.07 |     0.06 |
|                     TestStringFoldArray |   CaseFolding1 |   CaseFolding | 196.48 ns | 49.0810 ns | 144.7166 ns | 293.25 ns |   2.63 |     1.93 |
|                    TestStringFoldbyChar |   CaseFolding1 |   CaseFolding | 216.88 ns | 44.8394 ns | 129.3718 ns | 293.25 ns |   2.90 |     1.73 |
|                                         |                |               |           |            |             |           |        |          |
|                    **CoreFXCompareOrdinal** | **ЯЯЯЯЯЯЯЯЯЯЯЯЯ1** | **ЯЯЯЯЯЯЯЯЯЯЯЯЯ** |  **17.09 ns** |  **0.3828 ns** |   **0.6289 ns** |  **16.86 ns** |   **0.16** |     **0.01** |
| CoreFXCompareInvariantCultureIgnoreCase | ЯЯЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯЯЯ | 108.69 ns |  2.2223 ns |   4.4382 ns | 107.85 ns |   1.00 |     0.00 |
|                        ToLowerInvariant | ЯЯЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯЯЯ | 203.90 ns |  4.1304 ns |   6.4306 ns | 201.77 ns |   1.88 |     0.09 |
|                     TestStringFoldArray | ЯЯЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯЯЯ | 293.25 ns |  0.0000 ns |   0.0000 ns | 293.25 ns |   2.70 |     0.10 |
|                    TestStringFoldbyChar | ЯЯЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯЯЯ | 289.92 ns | 11.3498 ns |  31.2606 ns | 293.25 ns |   2.67 |     0.30 |
