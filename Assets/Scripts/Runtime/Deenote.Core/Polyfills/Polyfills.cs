using System.Collections;
using System.Collections.Generic;

namespace Deenote
{
    public static class Polyfills
    {
        public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
        {
            if (source is ICollection<T> gc) {
                count = gc.Count;
                return true;
            }
            if (source is ICollection c) {
                count = c.Count;
                return true;
            }
            count = 0;
            return false;
        }
    }
}