#nullable enable

using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library.Collections;
using System;
using UnityEngine;

namespace Deenote.Editing.Operations
{
    internal static partial class ChartOperations
    {
        private static void AddNoteEditorModel(this ChartEditorModel chart, NoteEditorModel note)
        {
            NoteCollisionHelpers.UpdateCollisionsPreAdding(chart, note);

            chart.Notes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note);
            chart.NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note);
            if (note.Tail is not null) {
                chart.NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note.Tail);
            }
        }

        internal static void AddNoteEditorModels(this ChartEditorModel chart, ReadOnlySpan<NoteEditorModel> notes)
        {
            // Optimize
            foreach (var note in notes) {
                chart.AddNoteEditorModel(note);
            }
        }

        internal static void RemoveNoteEditorModel(this ChartEditorModel chart, NoteEditorModel note)
        {
            chart.Notes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note);
            chart.NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note);
            if (note.Tail is not null) {
                chart.NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note.Tail);
            }

            NoteCollisionHelpers.UpdateCollisionsPostRemoving(chart, note);
        }

        internal static void RemoveNoteEditorModels(this ChartEditorModel chart, ReadOnlySpan<NoteEditorModel> notes)
        {
            // Optimize
            foreach (var note in notes) {
                chart.RemoveNoteEditorModel(note);
            }
        }

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
