using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace Deenote.Core
{
    public static class Asserts
    {
#pragma warning disable CS8777
        [Conditional("UNITY_ASSERTIONS")]
        public static void NotNull<T>([NotNull] T value, [CallerArgumentExpression("value")] string expression = "")
        {
            Debug.Assert(value != null, $"{expression} is null");

        }
#pragma warning restore CS8777

        [Conditional("UNITY_ASSERTIONS")]
        public static void InOrder<T, TComparer>(ReadOnlySpan<T> items, TComparer comparer, Func<T, int, T, int, string>? additionalMessage = null)
            where TComparer : IComparer<T>
            => InOrderImpl(items.ToArray(), comparer, additionalMessage);

        [Conditional("UNITY_ASSERTIONS")]
        public static void InOrder<T>(IEnumerable<T> items, IComparer<T> comparer, Func<T, int, T, int, string>? additionalMessage = null)
            => InOrderImpl(items, comparer, additionalMessage);

        private static void InOrderImpl<T>(IEnumerable<T> items, IComparer<T> comparer, Func<T, int, T, int, string>? additionalMessage = null)
        {
            using var enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext())
                return;

            var prev = enumerator.Current;
            var iPrev = 0;

            while (enumerator.MoveNext()) {
                var current = enumerator.Current;
                var iCurr = iPrev + 1;
                if (comparer.Compare(prev, current) > 0) {
                    Debug.Assert(false, additionalMessage?.Invoke(prev, iPrev, current, iCurr) ?? $"Items (#{iPrev}, #{iCurr}) are not in order");
                }
                prev = current;
                iPrev++;
            }
        }
    }
}
