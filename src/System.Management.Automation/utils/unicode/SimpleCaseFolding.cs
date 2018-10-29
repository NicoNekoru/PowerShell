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
            var summary = BenchmarkRunner.Run<IntroBenchmarkBaseline>();
        }
    }

    public class IntroBenchmarkBaseline
    {
        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void CoreFXCompareOrdinal(string a, string b)
        {
            string.Compare(a, b, StringComparison.Ordinal);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void CoreFXCompareOrdinalIgnoreCase(string a, string b)
        {
            string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void CoreFXCompareInvariantCulture(string a, string b)
        {
            string.Compare(a, b, StringComparison.InvariantCulture);
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(Data))]
        public void CoreFXCompareInvariantCultureIgnoreCase(string a, string b)
        {
            string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void CoreFXCompareCurrentCulture(string a, string b)
        {
            string.Compare(a, b, StringComparison.CurrentCulture);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void CoreFXCompareCurrentCultureIgnoreCase(string a, string b)
        {
            string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void CompareFolded(string a, string b)
        {
            SimpleCaseFolding.CompareFolded(a, b);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void ToLowerRu(string a, string b)
        {
            a.ToLower();
            b.ToLower();
        }

        static System.Globalization.CultureInfo rus = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void ToLowerInvariant(string a, string b)
        {
            a.ToLowerInvariant();
            b.ToLowerInvariant();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void TestStringFold(string a, string b)
        {
            a.Fold();
            b.Fold();
        }

        public IEnumerable<object[]> Data()
        {
            yield return new object[] { "CompareFolded1", "CompareFolded" };
        }
    }
}
