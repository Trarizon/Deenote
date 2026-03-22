#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Core.EditorModels
{
    internal sealed class NoteSelectionContext
    {
        public ProjectContext ProjectContext => EditorContext.ProjectContext;
        public EditorContext EditorContext { get; }

        internal readonly List<NoteModel> _selectedNotes = new();

        public ReadOnlySpan<NoteModel> SelectedNotes => _selectedNotes.AsSpan();

        public NoteSelectionContext(EditorContext editorContext)
        {
            EditorContext = editorContext;
        }

        public void SelectNote(NoteModel note)
        {
            if (note.IsSelected)
                return;

            SelectNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));
        }

        public void SelectNotes(ReadOnlySpan<NoteModel> notes)
        {
            RaiseSelectedNotesChanging();
            SelectNotesNonNotify(notes);
            RaiseSelectedNotesChanged();
        }

        public void ReselectNote(NoteModel note)
            => ReselectNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));

        public void ReselectNotes(ReadOnlySpan<NoteModel> notes)
        {
            RaiseSelectedNotesChanging();
            ClearSelectionNonNotify();
            SelectNotesNonNotify(notes);
            RaiseSelectedNotesChanged();
        }

        public void DeselectNote(NoteModel note)
            => DeselectNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));

        public void DeselectNotes(ReadOnlySpan<NoteModel> notes)
        {
            RaiseSelectedNotesChanging();
            DeselectNotesNonNotify(notes);
            RaiseSelectedNotesChanged();
        }

        public void ReplaceNotes(ReadOnlySpan<NoteModel> remove,  ReadOnlySpan<NoteModel> add)
        {
            RaiseSelectedNotesChanging();
            DeselectNotesNonNotify(remove);
            SelectNotesNonNotify(add);
            NoteComparers.AssertInTimeOrder(_selectedNotes);
            RaiseSelectedNotesChanged();
        }

        public void ClearSelection()
        {
            RaiseSelectedNotesChanging();
            ClearSelectionNonNotify();
            RaiseSelectedNotesChanged();
        }

        private void SelectNotesNonNotify(ReadOnlySpan<NoteModel> notes)
        {
            Debug.Assert(notes.ToArray().All(note => ProjectContext.CurrentChart?.NoteNodes.Contains(note) ?? false));
            _selectedNotes.GetSortedModifier(NoteComparers.NodeTime).AddRange(notes);
            foreach (var note in notes) {
                note.IsSelected = true;
            }
        }

        private void DeselectNotesNonNotify(ReadOnlySpan<NoteModel> notes)
        {
            foreach (var note in notes) {
                if (note.IsSelected) {
                    var rmv = _selectedNotes.Remove(note);
                    Debug.Assert(rmv is true);
                    note.IsSelected = false;
                }
            }
        }

        private void ClearSelectionNonNotify()
        {
            foreach (var note in _selectedNotes) {
                note.IsSelected = false;
            }
            _selectedNotes.Clear();
        }

        #region Events

        public event Action<NoteSelectionContext, SelectedNotesChangingEventArgs>? SelectedNotesChanging;
        public event Action<NoteSelectionContext, SelectedNotesChangedEventArgs>? SelectedNotesChanged;

        internal void RaiseSelectedNotesChanging()
        {
            SelectedNotesChanging?.Invoke(this, new SelectedNotesChangingEventArgs(_selectedNotes));
        }

        internal void RaiseSelectedNotesChanged()
        {
            SelectedNotesChanged?.Invoke(this, new SelectedNotesChangedEventArgs(_selectedNotes));
        }

        #endregion

        public readonly struct SelectedNotesChangingEventArgs
        {
            private readonly List<NoteModel> _notes;
            public ReadOnlySpan<NoteModel> SelectedNotes => _notes.AsSpan();
            internal SelectedNotesChangingEventArgs(List<NoteModel> notes) => _notes = notes;
        }

        public readonly struct SelectedNotesChangedEventArgs
        {
            private readonly List<NoteModel> _notes;
            public ReadOnlySpan<NoteModel> SelectedNotes => _notes.AsSpan();
            internal SelectedNotesChangedEventArgs(List<NoteModel> notes) => _notes = notes;
        }
    }
}
