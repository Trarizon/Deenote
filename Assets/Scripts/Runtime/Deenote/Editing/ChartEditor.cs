using Deenote.Core;
using Deenote.EditorModels;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using Deenote.Project;
using System;
using System.Collections.Immutable;

namespace Deenote.Editing
{
    public sealed class ChartEditor
    {
        private OperationMemento2 _operations;
        private ProjectManager2 _projectManager;
        private ChartNotesSelector _notesSelector;

        public ChartEditor(OperationMemento2 operations, ProjectManager2 projectManager)
        {
            _operations = operations;
            _projectManager = projectManager;
            _notesSelector = App.EditingManager.NotesSelector;

        }

        private void OnNoteCollectionChanged()
        {
            Asserts.InOrder(_projectManager.CurrentChart!.NoteNodes, NodeTimeUniqueComparer.Instance);
            App.GameStageManager.ForEditorUpdateNotes(true, false);
            App.GamePlayManager.ForEditorUpdateAudio(true, false);
        }

        public void AddNote(ChartEditorModel chart, NoteModel note)
        {
            var op = chart._model.AddNote(note)
                .OnRedone(note =>
                {
                    _notesSelector.ClearSelection();
                    OnNoteCollectionChanged();
                })
                .OnUndone(note => OnNoteCollectionChanged());
            _operations.Do(op);
        }

        public void AddNotes(ChartEditorModel chart, ReadOnlySpan<NoteModel> notes)
        {
            if (notes.IsEmpty)
                return;

            if (notes.Length == 1) {
                AddNote(chart, notes[0]);
                return;
            }

            var op = chart._model.AddMultipleNotes(ImmutableArray.Create(notes))
                .OnRedone(notes =>
                {
                    _notesSelector.Reselect(notes.AsSpan());
                    OnNoteCollectionChanged();
                })
                .OnUndone(notes =>
                {
                    _notesSelector.SelectedNotes.RemoveAll(notes, (ns, n) => ns.Contains(n));
                    OnNoteCollectionChanged();
                    Asserts.InOrder(chart.NoteNodes, NodeTimeUniqueComparer.Instance);
                });
            _operations.Do(op);
        }

        public void RemoveNotes(ChartEditorModel chart, ReadOnlySpan<NoteModel> notes)
        {
            if (notes.IsEmpty)
                return;

            var op = GetRemoveNotesOperation(chart, notes);
            _operations.Do(op);
        }

        public IUndoableOperation GetRemoveNotesOperation(ChartEditorModel chart, ReadOnlySpan<NoteModel> notes)
        {
            return chart._model.RemoveOrderedNotes(ImmutableArray.Create(notes))
                .OnRedone(notes =>
                {
                    _notesSelector.ClearSelection();
                    OnNoteCollectionChanged();
                })
                .OnUndone(notes =>
                {
                    _notesSelector.SelectedNotes.AddRange(notes.AsSpan());
                    OnNoteCollectionChanged();
                    Asserts.InOrder(chart.NoteNodes, NodeTimeUniqueComparer.Instance);
                });
        }
    }
}