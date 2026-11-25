using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.Core.GameStage;
using Deenote.CoreB;
using Deenote.EditorModels;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using System;
using System.Collections.Generic;

namespace Deenote.GameStage
{
    partial class GameStageManager
    {
        private readonly List<GameStageNoteController> _activeNotes = new();
        private readonly List<GameStageSpeedChangeWarningNoteController> _activeWarningNotes = new();
        private readonly List<PlacementNoteIndicatorController> _activeIndicators = new();

        internal ReadOnlySpan<GameStageNoteController> ActiveNotes => _activeNotes.AsSpan();
        internal ReadOnlySpan<GameStageSpeedChangeWarningNoteController> ActiveWarningNotes => _activeWarningNotes.AsSpan();

        private void Ctor_Notes()
        {
            NotesManager.ActiveNotesChanged += (s, e) =>
            {
                RefreshStageVisibleNotes(e.ActiveNotes);
            };
            WarningNotesManager.ActiveNotesChanged += (s, e) =>
            {
                RefreshStageVisibleWarningNotes(e.ActiveNotes);
            };

            _gamePlayManager.TimeChanged += e =>
            {
                if (App.ProjectManager.CurrentChart is null)
                    return;

                NotesManager.ShiftActiveNotes(!e.IsByJump);
                WarningNotesManager.ShiftActiveNotes();
            };
            _notePlacer.PrototypesChanged += s =>
            {
                RefreshStageNoteIndicators(s.Prototypes.AsSpan());
            };
        }

        private void RefreshStageVisibleNotes(ReadOnlySpan<NoteModel> notes)
        {
            App.Logger.LogDebug($"RefreshStageVisibleNotes: {notes.Length}");
            if (NoteFactory is null) {
                return;
            }

            foreach (var note in _activeNotes) {
                NoteFactory.Release(note);
            }
            _activeNotes.Clear();
            foreach (var note in notes) {
                var controller = NoteFactory.Create();
                controller.Initialize(note);
                _activeNotes.Add(controller);
            }

            Asserts.InOrder(notes, NodeTimeComparer.Instance);
            GameStageNoteController? prevNote = null;
            foreach (var note in _activeNotes) {
                note.PostInitialize(prevNote);
                prevNote = note;
            }
        }

        private void RefreshStageVisibleWarningNotes(ReadOnlySpan<SpeedChangeWarningModel> notes)
        {
            // App.Logger.LogDebug($"RefreshStageVisibleWarningNotes: {notes.Length}");
            if (NoteFactory is null) {
                return;
            }

            foreach (var note in _activeWarningNotes) {
                NoteFactory.Release(note);
            }
            _activeWarningNotes.Clear();
            foreach (var note in notes) {
                var controller = NoteFactory.CreateWarningNote();
                controller.Initialize(note);
                _activeWarningNotes.Add(controller);
            }

            Asserts.InOrder(notes, NodeTimeComparer.Instance);
        }

        private void RefreshStageNoteIndicators(ReadOnlySpan<NotePrototypeModel> notes)
        {
            if (NoteFactory is null) {
                return;
            }

            foreach (var indicator in _activeIndicators) {
                NoteFactory.Release(indicator);
            }
            _activeIndicators.Clear();
            foreach (var note in notes) {
                var indicator = NoteFactory.CreateIndicator();
                indicator.Initialize(note);
                _activeIndicators.Add(indicator);
            }

            Asserts.InOrder(notes, NodeTimeComparer.Instance);
        }

        public void ForEditorUpdateNotes(bool noteCollectionChangedOrNoteTimeRelatedPropertyChanged, bool notesVisualDataChanged)
        {
            switch (noteCollectionChangedOrNoteTimeRelatedPropertyChanged, notesVisualDataChanged) {
                case (true, false):
                    NotesManager.RefreshActiveNotes();
                    break;
                case (false, true):
                    foreach (var note in _activeNotes) {
                        note.RefreshVisual();
                    }
                    break;
                case (true, true):
                    NotesManager.RefreshActiveNotes();
                    break;
                default:
                    return;
            }
        }

        public void ForEditorUpdateWarningNotes()
        {
            WarningNotesManager.RefreshActiveNotes();
        }
    }
}
