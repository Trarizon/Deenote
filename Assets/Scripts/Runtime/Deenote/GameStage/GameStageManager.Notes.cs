using Deenote.Core;
using Deenote.Core.GameStage;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using System;
using System.Collections.Generic;

namespace Deenote.GameStage
{
    partial class GameStageManager
    {
        private readonly List<GameStageNoteController> _notes = new();
        private readonly List<GameStageSpeedChangeWarningNoteController> _warningNotes = new();

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

            App.GamePlayManager.TimeChanged += e =>
            {
                if (App.ProjectManager.CurrentChart is null)
                    return;

                NotesManager.ShiftActiveNotes(!e.IsByJump);
                WarningNotesManager.ShiftActiveNotes();
            };
        }

        private void RefreshStageVisibleNotes(ReadOnlySpan<NoteModel> notes)
        {
            App.Logger.LogDebug($"RefreshStageVisibleNotes: {notes.Length}");
            if (NoteFactory is null) {
                _notes.Clear();
                return;
            }

            foreach (var note in _notes) {
                NoteFactory.Release(note);
            }
            _notes.Clear();
            foreach (var note in notes) {
                var controller = NoteFactory.Create();
                controller.Initialize(note);
                _notes.Add(controller);
            }

            Asserts.InOrder(notes, NodeTimeComparer.Instance);
            GameStageNoteController? prevNote = null;
            foreach (var note in _notes) {
                note.PostInitialize(prevNote);
                prevNote = note;
            }
        }

        private void RefreshStageVisibleWarningNotes(ReadOnlySpan<SpeedChangeWarningModel> notes)
        {
            // App.Logger.LogDebug($"RefreshStageVisibleWarningNotes: {notes.Length}");
            if (NoteFactory is null) {
                _warningNotes.Clear();
                return;
            }

            foreach (var note in _warningNotes) {
                NoteFactory.Release(note);
            }
            _warningNotes.Clear();
            foreach (var note in notes) {
                var controller = NoteFactory.CreateWarningNote();
                controller.Initialize(note);
                _warningNotes.Add(controller);
            }

            Asserts.InOrder(notes, NodeTimeComparer.Instance);
        }
    }
}
