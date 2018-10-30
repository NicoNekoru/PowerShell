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
        public int CoreFXCompareOrdinal(string a, string b)
        {
            return string.Compare(a, b, StringComparison.Ordinal);
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public int CoreFXCompareOrdinalIgnoreCase(string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public int CoreFXCompareInvariantCulture(string a, string b)
        {
            return string.Compare(a, b, StringComparison.InvariantCulture);
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(Data))]
        public int CoreFXCompareInvariantCultureIgnoreCase(string a, string b)
        {
            return string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public int CoreFXCompareCurrentCulture(string a, string b)
        {
            return string.Compare(a, b, StringComparison.CurrentCulture);
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public int CoreFXCompareCurrentCultureIgnoreCase(string a, string b)
        {
            return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public int CompareFolded(string a, string b)
        {
            return SimpleCaseFolding.CompareFolded(a, b);
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public (string, string) ToLowerRussian(string a, string b)
        {
            return (a.ToLower(rus), b.ToLower(rus));
        }

        static System.Globalization.CultureInfo rus = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public (string, string) ToLowerInvariant(string a, string b)
        {
            return (a.ToLowerInvariant(), b.ToLowerInvariant());
        }

        //[Benchmark]
        //[ArgumentsSource(nameof(Data))]
        public (string, string) TestStringFold(string a, string b)
        {
            return (a.Fold(), b.Fold());
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public int TestStringFoldArray(string a, string b)
        {
            int ch = 0;
            var arr = SimpleCaseFolding.s_simpleCaseFoldingTableBMPane1;
            //var arr = arr1;
            for (int i = 0; i < a.Length; i++)
            {
                ch = arr[a[i]];
            }
            for (int i = 0; i < b.Length; i++)
            {
                ch = arr[b[i]];
            }
            return ch;
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public int TestStringFoldbyChar(string a, string b)
        {
            int ch = 0;
            var arr = SimpleCaseFolding.s_simpleCaseFoldingTableBMPane1;
            //var arr = arr1;
            for (int i = 0; i < a.Length; i++)
            {
                ch = SimpleCaseFolding.Fold(a[i]);
            }
            for (int i = 0; i < b.Length; i++)
            {
                ch = SimpleCaseFolding.Fold(b[i]);
            }
            return ch;
        }

        //public static readonly int[] arr1 = SimpleCaseFolding.s_simpleCaseFoldingTableBMPane1;

        public IEnumerable<object[]> Data()
        {
            yield return new object[] { "ЯЯЯЯЯЯЯЯЯЯЯЯЯ1", "ЯЯЯЯЯЯЯЯЯЯЯЯЯ" };
            yield return new object[] { "CaseFolding1", "CaseFolding" };
        }
    }
}
