using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Deenote.CoreB.Helpers
{
    public static class ListUtils
    {
        public static Span<T> AsSpan<T>(this List<T> list)
            => list is null ? Span<T>.Empty : Utils<T>.GetUnderlyingArray(list).AsSpan(..list.Count);

        public static void Replace<T>(this List<T> list, ReadOnlySpan<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }

        public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> collection)
        {
            foreach (var item in collection) {
                list.Add(item);
            }
        }

        public static void EnsureCapacity<T>(this List<T> list, int minCapacity)
        {
            if (minCapacity > list.Capacity)
                list.Capacity = minCapacity;
        }

        private static class Utils<T>
        {
            public static ref T[] GetUnderlyingArray(List<T> list)
            {
                var arr = Unsafe.As<List<T>, StrongBox<T[]>>(ref list);
                Debug.Assert(arr.Value is T[]);
                return ref arr.Value;
            }
        }
    }
}
