#nullable enable

using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.Editing.EditorModels;
using Deenote.Library.Collections;
using System;

namespace Deenote.Editing.Operations
{
    internal static partial class ChartOperations
    {
        internal static void AddBackgroundNoteEditorModels(this ChartEditorModel chart, ReadOnlySpan<BackgroundNoteEditorModel> notes)
        {
            chart.BackgroundNotes.GetSortedModifier(NoteComparers.ViaTime).AddRange(notes);
        }

        internal static void RemoveBackgroundNoteEditorModels(this ChartEditorModel chart, ReadOnlySpan<BackgroundNoteEditorModel> notes)
        {
            foreach (var note in notes) {
                chart.BackgroundNotes.GetSortedModifier(NoteComparers.ViaTime).Remove(note);
            }
        }

        internal static void AddWarningNoteEditorModels(this ChartEditorModel chart, ReadOnlySpan<WarningNoteEditorModel> notes)
        {
            chart.WarningNotes.GetSortedModifier(NoteComparers.ViaTime).AddRange(notes);
        }

        internal static void RemoveWarningNoteEditorModels(this ChartEditorModel chart, ReadOnlySpan<WarningNoteEditorModel> notes)
        {
            foreach (var note in notes) {
                chart.WarningNotes.GetSortedModifier(NoteComparers.ViaTime).Remove(note);
            }
        }
    }
}
