``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.10240.17146 (1507/RTM/Threshold1)
Intel Core i3-4150 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=3410090 Hz, Resolution=293.2474 ns, Timer=TSC
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|                                Method |         StrA |        StrB |      Mean |      Error |     StdDev |    Median | Allocated |
|-------------------------------------- |------------- |------------ |----------:|-----------:|-----------:|----------:|----------:|
|                  **CoreFXCompareOrdinal** | **CaseFolding1** | **CaseFolding** |  **15.96 ns** |  **0.3130 ns** |  **0.2613 ns** |  **15.90 ns** |       **0 B** |
|        CoreFXCompareOrdinalIgnoreCase | CaseFolding1 | CaseFolding |  27.97 ns |  0.6491 ns |  0.5754 ns |  27.80 ns |       0 B |
|         CoreFXCompareInvariantCulture | CaseFolding1 | CaseFolding |  77.40 ns |  1.9387 ns |  1.6189 ns |  76.93 ns |       0 B |
|           CoreFXCompareRussianCulture | CaseFolding1 | CaseFolding | 204.81 ns |  4.0018 ns |  3.3417 ns | 203.59 ns |       0 B |
| CoreFXCompareRussianCultureIgnoreCase | CaseFolding1 | CaseFolding | 213.71 ns | 13.0362 ns | 16.9508 ns | 204.92 ns |       0 B |
|                         CompareFolded | CaseFolding1 | CaseFolding |  31.09 ns |  0.6045 ns |  0.4720 ns |  30.89 ns |       0 B |
|                  **CoreFXCompareOrdinal** | **ЯЯЯЯЯЯЯЯЯЯЯ1** | **ЯЯЯЯЯЯЯЯЯЯЯ** |  **16.02 ns** |  **0.3506 ns** |  **0.3280 ns** |  **15.88 ns** |       **0 B** |
|        CoreFXCompareOrdinalIgnoreCase | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ |  35.66 ns |  0.7217 ns |  0.9879 ns |  35.31 ns |       0 B |
|         CoreFXCompareInvariantCulture | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ |  98.16 ns |  1.8872 ns |  1.8535 ns |  97.57 ns |       0 B |
|           CoreFXCompareRussianCulture | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ | 205.69 ns |  4.1180 ns |  3.6505 ns | 204.26 ns |       0 B |
| CoreFXCompareRussianCultureIgnoreCase | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ | 209.85 ns |  4.2032 ns | 10.5451 ns | 205.31 ns |       0 B |
|                         CompareFolded | ЯЯЯЯЯЯЯЯЯЯЯ1 | ЯЯЯЯЯЯЯЯЯЯЯ |  31.87 ns |  0.6672 ns |  1.7693 ns |  30.99 ns |       0 B |
