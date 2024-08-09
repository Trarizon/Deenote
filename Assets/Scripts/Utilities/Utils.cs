using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Deenote.Utilities
{
    public static class Utils
    {
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

        public static bool IsValidFileName(string fileName)
            => fileName.IndexOfAny(_invalidFileNameChars) < 0;

        public static bool IsValidPath(string path)
            => path.IndexOfAny(_invalidPathChars) < 0;

        public static bool EndsWithOneOf(this string str, ReadOnlySpan<string> ends)
        {
            foreach (var end in ends) {
                if (str.EndsWith(end))
                    return true;
            }
            return false;
        }

        public static bool IncAndTryWrap(this ref float value, float delta, float max)
        {
            value += delta;
            if (value > max) {
                value -= max;
                return true;
            }
            else {
                return false;
            }
        }

        public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, List<T> list)
        {
            if (span.Length != list.Count)
                return false;

            for (int i = 0; i < span.Length; i++) {
                if (!EqualityComparer<T>.Default.Equals(span[i], list[i]))
                    return false;
            }
            return true;
        }

        public static void RemoveRange<T>(this List<T> list, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            list.RemoveRange(offset, length);
        }

        public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return;

            var val = list[fromIndex];
            list.RemoveAt(fromIndex);
            list.Insert(toIndex, val);
        }

        public static T[] Array<T>(int length)
        {
            if (length == 0)
                return System.Array.Empty<T>();
            else
                return new T[length];
        }

        public static bool IsSameForAll<T, TValue>(this ListReadOnlyView<T> list, Func<T, TValue> valueGetter, out TValue value, IEqualityComparer<TValue> comparer = null)
        {
            switch (list) {
                case { IsNull: true }:
                case { Count: 0 }:
                    value = default;
                    return true;
                case { Count: 1 }:
                    value = valueGetter(list[0]);
                    return true;
            }

            value = valueGetter(list[0]);
            comparer ??= EqualityComparer<TValue>.Default;

            for (int i = 0; i < list.Count; i++) {
                if (!comparer.Equals(value, valueGetter(list[i]))) {
                    value = default;
                    return false;
                }
            }

            return true;
        }
    }
}