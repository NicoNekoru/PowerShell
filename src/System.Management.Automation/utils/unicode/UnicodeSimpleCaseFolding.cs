// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Management.Automation.Unicode
{
    /// <summary>
    /// </summary>
    public static partial class SimpleCaseFolding
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

                if (IsNotSurrogate(ch))
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNotSurrogate(char c)
        {
            return (c < HIGH_SURROGATE_START) || (c > LOW_SURROGATE_END);
        }

        /// <summary>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfFoldedIgnoreCase(this string source, char ch)
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
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            ref char refA = ref MemoryMarshal.GetReference(strA.AsSpan());
            ref char refB = ref MemoryMarshal.GetReference(strB.AsSpan());
            ref char simpleCaseFoldingTableBMPane1 = ref MemoryMarshal.GetReference(s_simpleCaseFoldingTableBMPane1.AsSpan());
            ref char simpleCaseFoldingTableBMPane2 = ref MemoryMarshal.GetReference(s_simpleCaseFoldingTableBMPane2.AsSpan());

            // -1 because char before last can be surrogate in both strings.
            var length = Math.Min(strA.Length, strB.Length) - 1;
            int i = 0;
            int c;

            for (; i < length; i++)
            {
                var c1 = Unsafe.Add(ref refA, i);
                var c2 = Unsafe.Add(ref refB, i);

                if (IsNotSurrogate(c1) && IsNotSurrogate(c2))
                {
                    c = Unsafe.Add(ref simpleCaseFoldingTableBMPane1, c1) - Unsafe.Add(ref simpleCaseFoldingTableBMPane1, c2);

                    if (c == 0)
                    {
                        continue;
                    }

                    return c;
                }

                if (IsNotSurrogate(c1) || IsNotSurrogate(c2))
                {
                    // Only one char is a surrogate
                    if (IsNotSurrogate(c1))
                    {
                        return -1;
                    }

                    return 1;
                }

                // Both char is surrogates
                var c12 = Unsafe.Add(ref refA, i + 1);
                var c22 = Unsafe.Add(ref refB, i + 1);

                // The index is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                var index1 = ((c1 - HIGH_SURROGATE_START) * 0x400) + (c12 - LOW_SURROGATE_START);
                // The utf32 is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                var utf32_1 = Unsafe.Add(ref simpleCaseFoldingTableBMPane2, index1);

                // The index is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                var index2 = ((c2 - HIGH_SURROGATE_START) * 0x400) + (c22 - LOW_SURROGATE_START);
                // The utf32 is Utf32 - 0x10000 (UNICODE_PLANE01_START)
                var utf32_2 = Unsafe.Add(ref simpleCaseFoldingTableBMPane2, index1);

                c = utf32_1 - utf32_2;

                if (c != 0)
                {
                    return c;
                }
            }

            // Last char shouldn't be a surrogate
            //c1 = Unsafe.Add(ref refA, i + 1);
            //c2 = Unsafe.Add(ref refB, i + 1);

            c = Unsafe.Add(ref simpleCaseFoldingTableBMPane1, Unsafe.Add(ref refA, i + 1)) - Unsafe.Add(ref simpleCaseFoldingTableBMPane1, Unsafe.Add(ref refB, i + 1));

            if (c != 0)
            {
                return c;
            }

            return strA.Length - strB.Length;
        }
    }
}
