// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace System.Management.Automation.Unicode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<IntroBenchmarkBaseline>();

            // Run: dotnet run -c release --AllCategories=StringFold
            // Run: dotnet run -c release --AllCategories=StringCompareFolded
            var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            Console.WriteLine("CaseFolding1".Fold());
            Console.WriteLine("ЯЯЯЯЯЯЯЯЯЯЯ1".Fold());
            Console.WriteLine(SimpleCaseFolding.CompareFolded("CaseFolding1", "ЯЯЯЯЯЯЯЯЯЯЯ1"));
        }
    }

    //[SimpleJob]
    [MemoryDiagnoser]
    public class IntroBenchmarkBaseline
    {
        //
        // Category: StringFold
        //
        [Benchmark(Baseline = true)]
        [BenchmarkCategory("StringCompareFolded")]
        [ArgumentsSource(nameof(Data))]
        public int CoreFXCompareOrdinal(string StrA, string StrB)
        {
            return string.Compare(StrA, StrB, StringComparison.Ordinal);
        }

        [Benchmark]
        [BenchmarkCategory("StringCompareFolded")]
        [ArgumentsSource(nameof(Data))]
        public int CoreFXCompareOrdinalIgnoreCase(string StrA, string StrB)
        {
            return string.Compare(StrA, StrB, StringComparison.OrdinalIgnoreCase);
        }

        [Benchmark]
        [BenchmarkCategory("StringCompareFolded")]
        [ArgumentsSource(nameof(Data))]
        public int CoreFXCompareInvariantCulture(string StrA, string StrB)
        {
            return string.Compare(StrA, StrB, StringComparison.InvariantCulture);
        }

        //[Benchmark(Baseline = true)]
        [BenchmarkCategory("StringCompareFolded")]
        //[ArgumentsSource(nameof(Data))]
        public int CoreFXCompareInvariantCultureIgnoreCase(string StrA, string StrB)
        {
            return string.Compare(StrA, StrB, StringComparison.InvariantCultureIgnoreCase);
        }

        [Benchmark]
        [BenchmarkCategory("StringCompareFolded")]
        [ArgumentsSource(nameof(Data))]
        public int CoreFXCompareRussianCulture(string StrA, string StrB)
        {
            return string.Compare(StrA, StrB, false, rus);
        }

        [Benchmark]
        [BenchmarkCategory("StringCompareFolded")]
        [ArgumentsSource(nameof(Data))]
        public int CoreFXCompareRussianCultureIgnoreCase(string StrA, string StrB)
        {
            return string.Compare(StrA, StrB, true, rus);
        }

        [Benchmark]
        [BenchmarkCategory("StringCompareFolded")]
        [ArgumentsSource(nameof(Data))]
        public int CompareFolded(string StrA, string StrB)
        {
            return SimpleCaseFolding.CompareFolded(StrA, StrB);
        }

        //
        // Category: StringFold
        //
        [Benchmark]
        [BenchmarkCategory("StringFold")]
        [ArgumentsSource(nameof(Data))]
        public (string, string) ToLowerRussian(string StrA, string StrB)
        {
            return (StrA.ToLower(rus), StrB.ToLower(rus));
        }

        static System.Globalization.CultureInfo rus = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

        //[Benchmark]
        [Benchmark(Baseline = true)]
        [BenchmarkCategory("StringFold")]
        [ArgumentsSource(nameof(Data))]
        public (string, string) ToLowerInvariant(string StrA, string StrB)
        {
            return (StrA.ToLowerInvariant(), StrB.ToLowerInvariant());
        }

        //[Benchmark]
        //[BenchmarkCategory("StringFold")]
        //[ArgumentsSource(nameof(Data))]
        public (string, string) TestStringFoldBase(string StrA, string StrB)
        {
            return (StrA.FoldBase(), StrB.FoldBase());
        }

        [Benchmark]
        [BenchmarkCategory("StringFold")]
        [ArgumentsSource(nameof(Data))]
        public (string, string) StringFold(string StrA, string StrB)
        {
            return (StrA.Fold(), StrB.Fold());
        }

        public IEnumerable<object[]> Data()
        {
            yield return new object[] { "CaseFolding1", "CaseFolding" };
            yield return new object[] { "ЯЯЯЯЯЯЯЯЯЯЯ1", "ЯЯЯЯЯЯЯЯЯЯЯ" };
        }
    }
}
