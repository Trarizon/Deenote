#nullable enable

using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.CoreB.Models.Notes.Comparing
{
    public static class NoteComparers
    {
        public static NoteTimeComparer ViaTime => NoteTimeComparer.Instance;

        public static void AssertInOrderViaTime(IEnumerable<INoteTime> notes, string additionalMessage)
            => AssertInOrder(notes, ViaTime, additionalMessage);

        private static void AssertInOrder<T>(IEnumerable<T> times, IComparer<T> comparer, string additionMessage)
        {
            using var enumerator = times.GetEnumerator();
            if (!enumerator.MoveNext())
                return;
            var prev = enumerator.Current;
            int iPrev = 0;

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                int iCurr = iPrev + 1;
                if (comparer.Compare(prev, curr) > 0) {
                    Debug.Assert(false, $"Notes (#{iPrev}, #{iCurr}) not in order: {additionMessage}");
                }
                prev = curr;
                iPrev = iCurr;
            }
        }
    }
}
