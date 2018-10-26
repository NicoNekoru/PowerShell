``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.10240.17146 (1507/RTM/Threshold1)
Intel Core i3-4150 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=3410090 Hz, Resolution=293.2474 ns, Timer=TSC
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|        Method |             a |             b |      Mean |     Error |    StdDev |    Median | Scaled | ScaledSD |
|-------------- |-------------- |-------------- |----------:|----------:|----------:|----------:|-------:|---------:|
| CoreFXCompare | CompareFolded | CompareFolded | 9.2850 ns | 0.2170 ns | 0.2584 ns | 9.1401 ns |   1.00 |     0.00 |
| CompareFolded | CompareFolded | CompareFolded | 0.9394 ns | 0.0576 ns | 0.1558 ns | 0.8538 ns |   0.10 |     0.02 |
