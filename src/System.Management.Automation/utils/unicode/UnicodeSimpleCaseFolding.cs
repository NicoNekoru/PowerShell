// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace System.Management.Automation.Unicode
{
    /// <summary>
    /// </summary>
    internal static partial class SimpleCaseFolding
    {
        /// <summary>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char Fold(char ch)
        {
            var index = s_simpleCaseFoldingTableSMPaneIn.BinarySearch(ch);

            if (index < 0)
            {
                return char.MinValue;
            }

            return (char)s_simpleCaseFoldingTableSMPaneOut[index];
        }

        /// <summary>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Fold(this string source)
        {
            return string.Create(source.Length, source , (chars, sourceString) =>
            {
                SpanFold(chars, sourceString);
            });
        }

        /// <summary>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fold(this Span<char> source)
        {
            SpanFold(source, source);
        }

        /// <summary>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<char> Fold(this ReadOnlySpan<char> source)
        {
            Span<char> result = new char[source.Length];

            SpanFold(result, source);

            return result;
        }

        internal const char HIGH_SURROGATE_START = '\ud800';
        internal const char HIGH_SURROGATE_END = '\udbff';
        internal const char LOW_SURROGATE_START = '\udc00';
        internal const char LOW_SURROGATE_END = '\udfff';
        internal const int  HIGH_SURROGATE_RANGE = 0x3FF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SpanFold(Span<char> result, ReadOnlySpan<char> source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                var ch = source[i];

                if (ch < HIGH_SURROGATE_START)
                {
                    result[i] = Fold(ch);
                }
            }
        }

        /// <summary>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfOrdinalIgnoreCase(this string source, char ch)
        {
            var foldedChar = Fold(ch);

            for (int i = 0; i < source.Length; i++)
            {
                if (Fold(source[i]) == foldedChar)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareFolded(this string strA, string strB)
        {
            if (object.ReferenceEquals(strA, strB))
            {
                return 0;
            }

            if (strA == null)
            {
                return -1;
            }

            if (strB == null)
            {
                return 1;
            }

            // -1 because char before last can be surrogate in both strings.
            var length = Math.Min(strA.Length, strB.Length) - 1;
            int i = 0;
            int c;

            for (; i < length; i++)
            {
                var c1 = strA[i];
                var c2 = strA[i];
                if ((c1 & HIGH_SURROGATE_START) != 0)
                {
                    if ((c2 & HIGH_SURROGATE_START) != 0)
                    {
                        // int32 search
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if ((c2 & HIGH_SURROGATE_START) != 0)
                    {
                        return -1;
                    }
                }

                c = Fold(strA[i]) - Fold(strA[i]);

                if (c != 0)
                {
                    return c;
                }
            }

            c = Fold(strA[i]) - Fold(strA[i]);

            if (c != 0)
            {
                return c;
            }

            return strA.Length - strB.Length;
        }
    }
}
