#nullable enable

using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Library.Collections;

namespace Deenote.Editing.EditorModels.Helpers
{
    public static class NoteDurationHelpers
    {
        internal static bool IsHold(this IGameNote note)
            => note.Kind is not NoteKind.Swipe && note.Duration > 0;

        internal static float GetActualDuration(this IGameNote note)
            => note.Kind is not NoteKind.Swipe ? note.Duration : 0;

        internal static void SetDuration(ChartEditorModel chart, NoteEditorModel note, float value)
        {
            switch (note.Duration, value) {
                case (0, > 0):
                    note.Duration = value;
                    chart.NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note.Tail!);
                    break;
                case ( > 0, 0):
                    chart.NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note.Tail!);
                    note.Duration = value;
                    break;
                case (0, 0):
                    break;
                default:
                    chart.NoteNodes.Sort(ModelComparers.ViaTimeUnique);
                    break;
            }
        }
    }
}
