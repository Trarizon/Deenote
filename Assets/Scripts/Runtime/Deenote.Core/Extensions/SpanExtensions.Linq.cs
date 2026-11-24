using System;
using System.Runtime.CompilerServices;

namespace Deenote.CoreB
{
    partial class SpanExtensions
    {
        public static OfTypeIterator<T, TResult> OfType<T, TResult>(this ReadOnlySpan<T> values)
            => new OfTypeIterator<T, TResult>(values);

        public ref struct OfTypeIterator<T, TResult>
        {
            private readonly ReadOnlySpan<T> _span;
            private int _index;

            internal OfTypeIterator(ReadOnlySpan<T> span)
            {
                _span = span;
                _index = -1;
            }

            public readonly OfTypeIterator<T, TResult> GetEnumerator() => this;

            public readonly ref readonly TResult Current => ref Unsafe.As<T, TResult>(ref Unsafe.AsRef(in _span[_index]));

            public bool MoveNext()
            {
                int index = _index + 1;
                while (index < _span.Length) {
                    if (_span[index] is TResult) {
                        _index = index;
                        return true;
                    }
                    index++;
                }
                _index = index;
                return false;
            }
        }
    }
}