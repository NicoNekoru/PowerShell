// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            return s_simpleCaseFoldingTableBMPane1[ch];

        }

        /// <summary>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Fold(this string source)
        {
            return string.Create(source.Length, source , (chars, sourceString) =>
            {
                SpanFold(chars, sourceString);
            });
        }

        /// <summary>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FoldBase(this string source)
        {
            /*
            var tmp = new char[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                tmp[i] = Fold(source[i]);
            }

            return tmp.ToString();
            */
            return string.Create(source.Length, source , (chars, sourceString) =>
            {
                SpanFoldBase(chars, sourceString);
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
/*        private static void SpanFoldBase(Span<char> result, ReadOnlySpan<char> source)
        {
            var length = source.Length;

            for (int i = 0; i < length; i++)
            {
                var ch = source[i];

                if (IsAscii(ch))
                {
                    if((uint)(ch - 'A') <= (uint)('Z' - 'A'))
                    {
                        result[i] = (char)(ch | 0x20);
                    }
                    else
                    {
                         result[i] = ch;
                    }

                    continue;
                }

                if (ch < HIGH_SURROGATE_START || ch > LOW_SURROGATE_END)
                {
                    result[i] = s_simpleCaseFoldingTableBMPane1[ch];
                }
                else
                {
                    if ((i + 1) < length)
                    {
                        var ch2 = source[i + 1];
                        if ((ch2 >= LOW_SURROGATE_START) && (ch2 <= LOW_SURROGATE_END))
                        {
                            // The index is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                            var index = ((ch - HIGH_SURROGATE_START) * 0x400) + (ch2 - LOW_SURROGATE_START);
                            // The utf32 is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                            var utf32 = s_simpleCaseFoldingTableBMPane2[index];
                            result[i] = (char)((utf32 / 0x400) + (int)HIGH_SURROGATE_START);
                            i++;
                            result[i] = (char)((utf32 % 0x400) + (int)LOW_SURROGATE_START);
                        }
                        else
                        {
                            // Broken unicode - throw?
                            result[i] = ch;
                        }
                    }
                    else
                    {
                        // Broken unicode - throw?
                        result[i] = ch;
                    }
                }
            }
        }
*/

        private static void SpanFoldBase(Span<char> result, ReadOnlySpan<char> source)
        {
            ref char res = ref MemoryMarshal.GetReference(result);
            ref char src = ref MemoryMarshal.GetReference(source);
            var simpleCaseFoldingTableBMPane1 = s_simpleCaseFoldingTableBMPane1.AsSpan();
            var simpleCaseFoldingTableBMPane2 = s_simpleCaseFoldingTableBMPane2.AsSpan();

            var length = source.Length;
            int i = 0;
            var ch = src;

            for (; i < length; i++)
            {
                //var ch = source[i];
                ch = Unsafe.Add(ref src, i);

                if (IsAscii(ch))
                {
                    if((uint)(ch - 'A') <= (uint)('Z' - 'A'))
                    {
                        //result[i] = (char)(ch | 0x20);
                        Unsafe.Add(ref res, i) = (char)(ch | 0x20);
                    }
                    else
                    {
                         //result[i] = ch;
                         Unsafe.Add(ref res, i) = ch;
                    }

                    continue;
                }

                if (ch < HIGH_SURROGATE_START || ch > LOW_SURROGATE_END)
                {
                    //result[i] = (char)s_simpleCaseFoldingTableBMPane1[ch];
                    //Unsafe.Add(ref res, i) = s_simpleCaseFoldingTableBMPane1[ch];
                    Unsafe.Add(ref res, i) = simpleCaseFoldingTableBMPane1[ch];
                }
                else
                {
                    if ((i + 1) < length)
                    {
                        var ch2 = Unsafe.Add(ref src, 1);
                        if ((ch2 >= LOW_SURROGATE_START) && (ch2 <= LOW_SURROGATE_END))
                        {
                            // The index is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                            var index = ((ch - HIGH_SURROGATE_START) * 0x400) + (ch2 - LOW_SURROGATE_START);
                            // The utf32 is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                            var utf32 = simpleCaseFoldingTableBMPane2[index];
                            Unsafe.Add(ref res, i) = (char)((utf32 / 0x400) + (int)HIGH_SURROGATE_START);
                            i++;
                            Unsafe.Add(ref res, i) = (char)((utf32 % 0x400) + (int)LOW_SURROGATE_START);
                        }
                        else
                        {
                            // Broken unicode - throw?
                            Unsafe.Add(ref res, i) = ch;
                        }
                    }
                    else
                    {
                        // Broken unicode - throw?
                        Unsafe.Add(ref res, i) = ch;
                    }
                }
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SpanFold(Span<char> result, ReadOnlySpan<char> source)
        {
            ref char res = ref MemoryMarshal.GetReference(result);
            ref char src = ref MemoryMarshal.GetReference(source);
            //var simpleCaseFoldingTableBMPane1 = s_simpleCaseFoldingTableBMPane1.AsSpan();
            //var simpleCaseFoldingTableBMPane2 = s_simpleCaseFoldingTableBMPane2.AsSpan();
            ref char simpleCaseFoldingTableBMPane1 = ref MemoryMarshal.GetReference(s_simpleCaseFoldingTableBMPane1.AsSpan());
            ref char simpleCaseFoldingTableBMPane2 = ref MemoryMarshal.GetReference(s_simpleCaseFoldingTableBMPane2.AsSpan());

            var length = source.Length;
            int i = 0;
            var ch = src;

            for (; i < length; i++)
            {
                //var ch = source[i];
                ch = Unsafe.Add(ref src, i);

                if (IsAscii(ch))
                {
                    if((uint)(ch - 'A') <= (uint)('Z' - 'A'))
                    {
                        //result[i] = (char)(ch | 0x20);
                        Unsafe.Add(ref res, i) = (char)(ch | 0x20);
                    }
                    else
                    {
                         //result[i] = ch;
                         Unsafe.Add(ref res, i) = ch;
                    }

                    continue;
                }

                if (ch < HIGH_SURROGATE_START || ch > LOW_SURROGATE_END)
                {
                    //result[i] = (char)s_simpleCaseFoldingTableBMPane1[ch];
                    //Unsafe.Add(ref res, i) = s_simpleCaseFoldingTableBMPane1[ch];
                    //Unsafe.Add(ref res, i) = simpleCaseFoldingTableBMPane1[ch];
                    Unsafe.Add(ref res, i) = Unsafe.Add(ref simpleCaseFoldingTableBMPane1, ch);
                }
                else
                {
                    if ((i + 1) < length)
                    {
                        var ch2 = Unsafe.Add(ref src, 1);
                        if ((ch2 >= LOW_SURROGATE_START) && (ch2 <= LOW_SURROGATE_END))
                        {
                            // The index is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                            var index = ((ch - HIGH_SURROGATE_START) * 0x400) + (ch2 - LOW_SURROGATE_START);
                            // The utf32 is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                            var utf32 = Unsafe.Add(ref simpleCaseFoldingTableBMPane2, index);
                            Unsafe.Add(ref res, i) = (char)((utf32 / 0x400) + (int)HIGH_SURROGATE_START);
                            i++;
                            Unsafe.Add(ref res, i) = (char)((utf32 % 0x400) + (int)LOW_SURROGATE_START);
                        }
                        else
                        {
                            // Broken unicode - throw?
                            Unsafe.Add(ref res, i) = ch;
                        }
                    }
                    else
                    {
                        // Broken unicode - throw?
                        Unsafe.Add(ref res, i) = ch;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAscii(char c)
        {
            return c < 0x80;
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
