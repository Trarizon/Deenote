using Deenote.Core.GameStage;
using Deenote.Entities.Models;
using Deenote.Helpers;
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
            Asserts.NotesInOrderViaTimeUnique(notes);
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
            Asserts.NotesInOrderViaTime(notes);
        }
    }
}
