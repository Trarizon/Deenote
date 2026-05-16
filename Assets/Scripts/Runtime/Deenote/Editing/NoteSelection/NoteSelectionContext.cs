#nullable enable

using Deenote.Contexts;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Editing.NoteSelection
{
    public sealed class NoteSelectionContext : INotifyPropertyChanging<NoteSelectionContext>, INotifyPropertyChanged<NoteSelectionContext>
    {
        public ProjectContext ProjectContext => EditorContext.ProjectContext;
        public EditorContext EditorContext { get; }

        internal readonly List<NoteEditorModel> _selectedNotes = new();

        public ReadOnlySpan<NoteEditorModel> SelectedNotes => _selectedNotes.AsSpan();

        public NoteSelectionContext(EditorContext editorContext)
        {
            EditorContext = editorContext;

            ProjectContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    _selectedNotes.Clear();
                }
            });
        }

        public void SelectNote(NoteEditorModel note)
        {
            if (note.IsSelected)
                return;

            SelectNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));
        }

        public void SelectNotes(ReadOnlySpan<NoteEditorModel> notes)
        {
            RaiseSelectedNotesChanging();
            SelectNotesNonNotify(notes);
            RaiseSelectedNotesChanged();
        }

        public void ReselectNote(NoteEditorModel note)
            => ReselectNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));

        public void ReselectNotes(ReadOnlySpan<NoteEditorModel> notes)
        {
            RaiseSelectedNotesChanging();
            ClearSelectionNonNotify();
            SelectNotesNonNotify(notes);
            RaiseSelectedNotesChanged();
        }

        public void DeselectNote(NoteEditorModel note)
            => DeselectNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));

        public void DeselectNotes(ReadOnlySpan<NoteEditorModel> notes)
        {
            RaiseSelectedNotesChanging();
            DeselectNotesNonNotify(notes);
            RaiseSelectedNotesChanged();
        }

        public void ReplaceNotes(ReadOnlySpan<NoteEditorModel> remove, ReadOnlySpan<NoteEditorModel> add)
        {
            RaiseSelectedNotesChanging();
            DeselectNotesNonNotify(remove);
            SelectNotesNonNotify(add);
            ModelAsserts.AssertInOrderViaTimeUnique(_selectedNotes);
            RaiseSelectedNotesChanged();
        }

        public void ClearSelection()
        {
            RaiseSelectedNotesChanging();
            ClearSelectionNonNotify();
            RaiseSelectedNotesChanged();
        }

        private void SelectNotesNonNotify(ReadOnlySpan<NoteEditorModel> notes)
        {
            Debug.Assert(notes.ToArray().All(note => ProjectContext.CurrentChart?.NoteNodes.Contains(note) ?? false));
            _selectedNotes.GetSortedModifier(ModelComparers.ViaTimeUnique).AddRange(notes);
            foreach (var note in notes) {
                note.IsSelected = true;
            }
        }

        private void DeselectNotesNonNotify(ReadOnlySpan<NoteEditorModel> notes)
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
        public event Action<NoteSelectionContext, PropertyEventArgs>? PropertyChanging;
        public event Action<NoteSelectionContext, PropertyEventArgs>? PropertyChanged;

        internal void RaiseSelectedNotesChanging()
        {
            SelectedNotesChanging?.Invoke(this, new SelectedNotesChangingEventArgs(_selectedNotes));
            PropertyChanging?.Invoke(this, new PropertyEventArgs(nameof(SelectedNotes)));
        }

        internal void RaiseSelectedNotesChanged()
        {
            SelectedNotesChanged?.Invoke(this, new SelectedNotesChangedEventArgs(_selectedNotes));
            PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(SelectedNotes)));
        }

        #endregion

        public readonly struct SelectedNotesChangingEventArgs
        {
            private readonly List<NoteEditorModel> _notes;
            public ReadOnlySpan<NoteEditorModel> SelectedNotes => _notes.AsSpan();
            internal SelectedNotesChangingEventArgs(List<NoteEditorModel> notes) => _notes = notes;
        }

        public readonly struct SelectedNotesChangedEventArgs
        {
            private readonly List<NoteEditorModel> _notes;
            public ReadOnlySpan<NoteEditorModel> SelectedNotes => _notes.AsSpan();
            internal SelectedNotesChangedEventArgs(List<NoteEditorModel> notes) => _notes = notes;
        }
    }
}
