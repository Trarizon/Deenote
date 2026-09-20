using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.Helpers
{
    public static partial class Asserts
    {
        [Conditional("UNITY_ASSERTIONS")]
        public static void NotesInOrderViaTimeUnique(IEnumerable<IStageNoteNode> notes, string? additionalMessage = null)
            => AssertInOrder(notes, NodeTimeUniqueComparer.Instance, additionalMessage);

        [Conditional("UNITY_ASSERTIONS")]
        public static void NotesInOrderViaTimeUnique<T>(ReadOnlySpan<T> notes, string? additionalMessage = null)
            where T : class, IStageNoteNode
            => AssertInOrder(notes.ToArray(), NodeTimeUniqueComparer.Instance, additionalMessage);


        private static void AssertInOrder<T>(IEnumerable<T> times, IComparer<T> comparer, string? additionalMessage = null)
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
                    UnityEngine.Debug.Assert(false, $"Notes (#{iPrev}, #{iCurr}) not in order: {additionalMessage}");
                }
                prev = curr;
                iPrev = iCurr;
            }
        }
    }
}
