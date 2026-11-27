using Deenote.Core.Helpers;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEngine;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace Deenote.CoreB.Notification.Collections
{
    public sealed class ObservableList<T> : INotifyCollectionChangedResettable<ObservableList<T>, T>, IList<T>, IReadOnlyList<T>
    {
        public event CollectionChangedEventHandler<ObservableList<T>, T>? CollectionChanged;

        // |---------------|
        // the items are:
        // [.._skip) [_skip + _skipLength.._count)

        private T[] _array;
        private int _count;
        private int _skip;
        private int _skipLength;

        public ObservableList()
        {
            _array = Array.Empty<T>();
            _count = 0;
        }

        public ObservableList(int capacity)
        {
            _array = new T[capacity];
            _count = 0;
        }

        public ObservableList(IEnumerable<T> items) : this()
        {
            AddRange(items);
        }

        public int Count => _count - _skipLength;

        public T this[int index]
        {
            get {
                if ((uint)index >= (uint)Count)
                    ThrowArgumentOutOfRangeException(nameof(index));

                if (_skipLength == 0)
                    return _array[index];

                if (index < _skip)
                    return _array[index];
                return _array[index + _skipLength];
            }
            set {
                AssertSequential();

                if ((uint)index >= (uint)Count)
                    ThrowArgumentOutOfRangeException(nameof(index));

                var oldItem = _array[index];
                _array[index] = value;
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Replace(index, oldItem, value));
            }
        }

        public ReadOnlySpan<T> AsSpan()
        {
            if (_skipLength != 0)
                throw new InvalidOperationException("Cannot get span while collection is not in sequential state");

            return _array.AsSpan(0, _count);
        }

        public void Add(T item)
        {
            AssertSequential();

            var index = _count;
            AddNonNotify(item);
            CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, item));
        }

        private void AddNonNotify(T item)
        {
            AssertSequential();

            if (_count == _array.Length)
                ArrayHelpers.Grow(ref _array, _count, _count + 1);

            _array[_count++] = item;
        }

        public void AddRange(IEnumerable<T> items)
        {
            AssertSequential();

            var index = _count;
            if (items.TryGetNonEnumeratedCount(out var count)) {
                if (count == 0)
                    return;

                ArrayHelpers.Grow(ref _array, _count, _count + count);
                if (items is ICollection<T> gc) {
                    gc.CopyTo(_array, _count);
                    _count += count;
                }
                else {
                    foreach (var item in items)
                        _array[_count++] = item;
                }
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, _array.AsSpan(index, count)));
                return;
            }

            foreach (var item in items)
                AddNonNotify(item);

            if (_count > index) {
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, _array.AsSpan(index, _count - index)));
            }
        }

        public void AddRange(ReadOnlySpan<T> items)
        {
            AssertSequential();

            if (items.IsEmpty)
                return;
            var index = _count;
            if (_count + items.Length > _array.Length)
                ArrayHelpers.Grow(ref _array, _count, _count + items.Length);
            items.CopyTo(_array.AsSpan(_count, items.Length));
            _count += items.Length;

            CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, items));
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(index));

            AssertSequential();
            InsertNonNotify(index, item);
            CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, item));
        }

        private void InsertNonNotify(int index, T item)
        {
            Debug.Assert((uint)index <= (uint)Count);
            AssertSequential();

            if (_count == _array.Length)
                ArrayHelpers.GrowForInsertion(ref _array, index, 1, _count);
            else
                Array.Copy(_array, index, _array, index + 1, _count - index);
            _array[index] = item;
            _count++;
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            if ((uint)index > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(index));

            AssertSequential();

            if (ReferenceEquals(items, this)) {
                InsertRange(index, _array.AsSpan(0, _count));
                return;
            }

            if (items.TryGetNonEnumeratedCount(out var count)) {
                if (count == 0)
                    return;

                if (_count + count > _array.Length)
                    ArrayHelpers.GrowForInsertion(ref _array, index, count, _count);
                else
                    Array.Copy(_array, index, _array, index + count, _count - index);

                if (items is ICollection<T> gc) {
                    gc.CopyTo(_array, index);
                    _count += count;
                }
                else {
                    var idx = index;
                    foreach (var item in items)
                        _array[idx++] = item;
                    _count += count;
                }
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, _array.AsSpan(index, count)));
                return;
            }
            else {
                var idx = index;
                foreach (var item in items)
                    InsertNonNotify(idx++, item);

                if (idx > index) {
                    CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, _array.AsSpan(index, idx - index)));
                }
            }
        }

        public void InsertRange(int index, ReadOnlySpan<T> items)
        {
            if ((uint)index > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(index));

            AssertSequential();

            if (items.IsEmpty)
                return;

            T[]? rented = null;
            if (_count + items.Length > _array.Length) {
                ArrayHelpers.GrowForInsertion(ref _array, index, items.Length, _count);
            }
            else {
                // if 'items' is part of current _array, moving items in array later may change the content of 'items', so we need to cache it
                // if grow triggered, items' won't be part of the new _array, so we needn't do this check
                if (_array.AsSpan().Overlaps(items)) {
                    rented = ArrayPool<T>.Shared.Rent(items.Length);
                    items.CopyTo(rented);
                    items = rented.AsSpan(0, items.Length);
                }

                Array.Copy(_array, index, _array, index + items.Length, _count - index);
            }


            items.CopyTo(_array.AsSpan(index, items.Length));
            _count += items.Length;

            try {
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Add(index, items));
            } finally {
                if (rented != null) {
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                        Array.Clear(rented, 0, items.Length);
                    }
                    ArrayPool<T>.Shared.Return(rented);
                }
            }
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(index));

            AssertSequential();

            _skip = index;
            _skipLength = 1;
            try {
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Remove(index, _array[index]));
            } finally {
                var restore = _skip + _skipLength;
                Array.Copy(_array, restore, _array, _skip, _count - restore);
                _skip = 0;
                _skipLength = 0;
                _count--;
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                    _array[_count] = default!;
                }
            }
        }

        public bool Remove(T item)
        {
            AssertSequential();

            var index = IndexOf(item);
            if (index >= 0) {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveRange(int start, int length)
        {
            if (start < 0 || length < 0 || (uint)start + (uint)length > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(start));

            AssertSequential();

            if (length == 0)
                return;

            _skip = start;
            _skipLength = length;
            try {
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Remove(start, _array.AsSpan(start, length)));
            } finally {
                var restore = _skip + _skipLength;
                Array.Copy(_array, restore, _array, _skip, _count - restore);
                _skip = 0;
                _skipLength = 0;
                _count -= length;
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                    Array.Clear(_array, _count, length);
                }
            }
        }

        /// <summary>
        /// NOTE: This method may invoke CollectionChanged multiple times.
        /// if one listener throws exception, CollectionChanged won't be invoked later, but the remove operation will continue.
        /// </summary>
        /// <param name="predicate">If predicate throws, the collection may be left in an undefined state</param>
        public void RemoveAll<TState>(TState state, Func<TState, T, bool> predicate)
        {
            AssertSequential();

            var count = 0;

            while (count < _count && !predicate(state, _array[count]))
                count++;

            if (count == _count)
                return;

            var ptr = count + 1;
            var notify = 1;

            ExceptionDispatchInfo? ex = null;

            while (ptr < _count) {
                if (predicate(state, _array[ptr])) {
                    ptr++;
                    notify++;
                    continue;
                }

                _skip = count;
                _skipLength = ptr - count;
                if (ex is null) {
                    try {
                        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Remove(count, _array.AsSpan(ptr - notify, notify)));
                    } catch (Exception e) {
                        ex = ExceptionDispatchInfo.Capture(e);
                    }
                }
                _array[count++] = _array[ptr++];
                _skipLength = 0;
                notify = 0;
            }

            _count = count;
            _skip = 0;
            _skipLength = 0;

            try {
                ex?.Throw();

                if (notify > 0)
                    CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Remove(count, _array.AsSpan(ptr - notify, notify)));
            } finally {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                    Array.Clear(_array, count, ptr - count);
                }
            }
        }

        /// <inheritdoc cref="RemoveAll{TState}(TState, Func{TState, T, bool})"/>
        public void RemoveAll(Func<T, bool> predicate)
            => RemoveAll(predicate, static (s, v) => s(v));

        public void Clear()
        {
            AssertSequential();

            if (_count == 0)
                return;

            var count = _count;
            _count = 0;

            try {
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Remove(0, _array.AsSpan(0, count)));
            } finally {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                    Array.Clear(_array, 0, count);
                }
            }
        }

        public void Replace(int index, int length, ReadOnlySpan<T> items)
        {
            if (index < 0 || length < 0 || (uint)index + (uint)length > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(index));

            AssertSequential();

            if (length == 0 && items.IsEmpty)
                return;

            // | before | replaced | after | cache
            // | index  | length,items | . | length

            T[]? rented = null;

            var requiredCount = _count + Math.Max(length, items.Length);
            if (requiredCount > _array.Length) {
                ArrayHelpers.Grow(ref _array, _count, requiredCount);
            }
            else {
                // if 'items' is part of current _array, moving items in array later may change the content of 'items', so we need to cache it
                // if grow triggered, items' won't be part of the new _array, so we needn't do this check
                if (_array.AsSpan().Overlaps(items)) {
                    rented = ArrayPool<T>.Shared.Rent(items.Length);
                    items.CopyTo(rented);
                    items = rented.AsSpan(0, items.Length);
                }
            }

            // | before | old | after | ...
            Array.Copy(_array, index, _array, requiredCount - length, length);
            // | before | old | after | old
            if (length != items.Length) {
                var afterLength = _count - index - length;
                Array.Copy(_array, index + length, _array, index + items.Length, afterLength);
            }
            // | before | old | after | old
            items.CopyTo(_array.AsSpan(index, items.Length));
            _count = _count + items.Length - length;

            try {
                CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Replace(index, _array.AsSpan(index, items.Length), _array.AsSpan(requiredCount - length, length)));
            } finally {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                    Array.Clear(_array, _count, requiredCount - _count);
                }
                if (rented != null) {
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                        Array.Clear(rented, 0, items.Length);
                    }
                    ArrayPool<T>.Shared.Return(rented);
                }
            }
        }

        // Optimize
        public void Replace(ReadOnlySpan<T> items)
            => Replace(0, _count, items);

        public void Move(int from, int to, int length)
        {
            if (from < 0 || to < 0 || length < 0 || (uint)from + (uint)length > (uint)Count || (uint)to + (uint)length > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(from));

            AssertSequential();

            if (length == 0 || from == to)
                return;

            MoveTo(_array.AsSpan(0, _count), from, to, length);
            CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Move(from, to, _array.AsSpan(to, length)));
        }

        private static void MoveTo(Span<T> span, int fromIndex, int toIndex, int length)
        {
            // Throws.ThrowIfNegative(fromIndex);
            // Throws.ThrowIfNegative(toIndex);

            if (fromIndex > toIndex) {
                // Throws.ThrowIfGreaterThan(fromIndex + length, span.Length);
                Core(span, toIndex, toIndex + length, length, fromIndex - toIndex);
            }
            else {
                // Throws.ThrowIfGreaterThan(toIndex + length, span.Length);
                Core(span, fromIndex, toIndex, toIndex - fromIndex, length);
            }

            static void Core(Span<T> span, int from, int to, int dist, int length)
            {
                Debug.Assert(to - from == dist);
                Debug.Assert(dist > 0);

                if (dist < length) {
                    var array = ArrayPool<T>.Shared.Rent(dist);
                    try {
                        var buffer = array.AsSpan(..dist);
                        span.Slice(from + length, dist).CopyTo(buffer);
                        span.Slice(from, length).CopyTo(span.Slice(to, length));
                        buffer.CopyTo(span.Slice(from, dist));
                    } finally {
                        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                            Array.Clear(array, 0, dist);
                        ArrayPool<T>.Shared.Return(array);
                    }
                }
                else {
                    var array = ArrayPool<T>.Shared.Rent(length);
                    try {
                        var buffer = array.AsSpan(..length);
                        span.Slice(from, length).CopyTo(buffer);
                        span.Slice(from + length, dist).CopyTo(span.Slice(from, dist));
                        buffer.CopyTo(span.Slice(to, length));
                    } finally {
                        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                            Array.Clear(array, 0, length);
                        ArrayPool<T>.Shared.Return(array);
                    }
                }
            }
        }

        public void Sort(IComparer<T>? comparer = null)
            => Sort(0, _count, comparer);

        public void Sort(int start, int count, IComparer<T>? comparer = null)
        {
            AssertSequential();

            if (start < 0 || count < 0 || (uint)start + (uint)count > (uint)Count)
                ThrowArgumentOutOfRangeException(nameof(start));

            if ((uint)count <= 1)
                return;

            Array.Sort(_array, start, count, comparer);
            CollectionChanged?.Invoke(this, CollectionChangedEventArgs<T>.Sort(start, _array.AsSpan(start, count)));
        }

        [Conditional("DEBUG")]
        private void AssertSequential()
        {
            Debug.Assert(_skipLength == 0);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowArgumentOutOfRangeException(string parameterName)
            => throw new ArgumentOutOfRangeException(parameterName);

        public int IndexOf(T item)
        {
            if (_skipLength == 0)
                return Array.IndexOf(_array, item, 0, _count);

            for (int i = 0; i < _skip; i++) {
                if (EqualityComparer<T>.Default.Equals(_array[i], item))
                    return i;
            }
            for (int i = _skip + _skipLength; i < _count; i++) {
                if (EqualityComparer<T>.Default.Equals(_array[i], item))
                    return i - _skipLength;
            }
            return -1;
        }

        public bool Contains(T item)
        {
            if (_skipLength == 0)
                return Array.IndexOf(_array, item, 0, _count) >= 0;

            for (int i = 0; i < _skip; i++) {
                if (EqualityComparer<T>.Default.Equals(_array[i], item))
                    return true;
            }
            for (int i = _skip + _skipLength; i < _count; i++) {
                if (EqualityComparer<T>.Default.Equals(_array[i], item))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_skipLength == 0) {
                Array.Copy(_array, 0, array, arrayIndex, _count);
            }
            else {
                var restore = _skip + _skipLength;
                Array.Copy(_array, restore, array, arrayIndex + _skip, _count - restore);
                Array.Copy(_array, 0, array, arrayIndex, _skip);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _skip; i++) {
                yield return _array[i];
            }
            for (int i = _skip + _skipLength; i < _count; i++) {
                yield return _array[i];
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        CollectionChangedEventArgs<T> INotifyCollectionChangedResettable<T>.GetResetEventArgs()
        {
            return CollectionChangedEventArgs<T>.Reset(_array.AsSpan(0, _count));
        }
    }
}
