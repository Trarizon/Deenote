#nullable enable

using System;

namespace Deenote.Library.Collections;
public static partial class SpanUtils
{
    public static T ElementAtOrDefault<T>(this ReadOnlySpan<T> span, int index)
    {
        if (index >= 0 && index < span.Length)
        {
            return span[index];
        }
        return default!;
    }
}