``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.10240.17146 (1507/RTM/Threshold1)
Intel Core i3-4150 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=3410090 Hz, Resolution=293.2474 ns, Timer=TSC
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|                                Method |         StrA |        StrB |      Mean |     Error |     StdDev |    Median | Scaled | ScaledSD | Allocated |
|-------------------------------------- |------------- |------------ |----------:|----------:|-----------:|----------:|-------:|---------:|----------:|
|                  **CoreFXCompareOrdinal** | **CaseFolding1** | **CaseFolding** |  **16.85 ns** | **0.3974 ns** |  **0.6303 ns** |  **16.67 ns** |   **1.00** |     **0.00** |       **0 B** |
|        CoreFXCompareOrdinalIgnoreCase | CaseFolding1 | CaseFolding |  28.69 ns | 0.6085 ns |  1.1428 ns |  28.14 ns |   1.70 |     0.09 |       0 B |
|         CoreFXCompareInvariantCulture | CaseFolding1 | CaseFolding |  89.84 ns | 2.7482 ns |  7.9731 ns |  88.84 ns |   5.34 |     0.51 |       0 B |
|           CoreFXCompareRussianCulture | CaseFolding1 | CaseFolding | 216.16 ns | 5.1994 ns | 14.8342 ns | 209.80 ns |  12.85 |     0.98 |       0 B |
| CoreFXCompareRussianCultureIgnoreCase | CaseFolding1 | CaseFolding | 206.85 ns | 4.1628 ns |  5.2646 ns | 205.35 ns |  12.29 |     0.52 |       0 B |
|                         CompareFolded | CaseFolding1 | CaseFolding |  23.33 ns | 0.4876 ns |  0.5218 ns |  23.23 ns |   1.39 |     0.06 |       0 B |
|                                       |              |             |           |           |            |           |        |          |           |
|                  **CoreFXCompareOrdinal** | **ЯЯЯЯЯЯЯЯЯЯЯ1** | **ЯЯЯЯЯЯЯЯЯЯЯ** |  **16.57 ns** | **0.3967 ns** |  **0.9505 ns** |  **16.31 ns** |   **1.00** |     **0.00** |       **0 B** |
|        CoreFXCompareOrdinalIgnoreCase | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ |  35.89 ns | 0.7396 ns |  1.2356 ns |  35.28 ns |   2.17 |     0.13 |       0 B |
|         CoreFXCompareInvariantCulture | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ |  98.38 ns | 1.9520 ns |  2.1697 ns |  97.41 ns |   5.95 |     0.33 |       0 B |
|           CoreFXCompareRussianCulture | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ | 208.95 ns | 4.6181 ns |  8.4444 ns | 205.06 ns |  12.64 |     0.82 |       0 B |
| CoreFXCompareRussianCultureIgnoreCase | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ | 206.56 ns | 4.0916 ns |  5.3202 ns | 205.15 ns |  12.50 |     0.72 |       0 B |
|                         CompareFolded | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ |  23.46 ns | 0.8557 ns |  1.1995 ns |  22.89 ns |   1.42 |     0.10 |       0 B |
