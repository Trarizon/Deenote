#nullable enable

using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.Editing.EditorModels.Comparing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote.Editing.EditorModels.Assertions
{
    internal static class ModelAsserts
    {
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertChartEditorModel(ChartEditorModel chart)
        {
            AssertInOrderViaTimeUnique(chart.Notes);
            AssertInOrder(chart.NoteNodes, NoteComparers.ViaTime);
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertInOrderViaTimeUnique(IEnumerable<INoteTimeUnique> notes, string? additionalMessage = null)
            => AssertInOrder(notes, ModelComparers.ViaTimeUnique, additionalMessage);

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertInOrderViaTimeUnique<T>(ReadOnlySpan<T> notes, string? additionalMessage = null)
            where T : class, INoteTimeUnique
            => AssertInOrder(notes.ToArray(), ModelComparers.ViaTimeUnique, additionalMessage);

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
                    Debug.Assert(false, $"Notes (#{iPrev}, #{iCurr}) not in order: {additionalMessage}");
                }
                prev = curr;
                iPrev = iCurr;
            }
        }
    }
}
