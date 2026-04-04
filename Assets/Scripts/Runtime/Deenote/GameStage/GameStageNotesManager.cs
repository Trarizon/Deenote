#nullable enable

using Deenote.Core.GameStage;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels.Assertions;
using System.Collections.Generic;

namespace Deenote.GameStage
{
    internal sealed class GameStageNotesManager
    {
        private readonly GameStageContext _stage;
        private readonly GameStageNotesContext _context;
        private IGameStageNoteFactory? _factory;

        private List<GameStageNoteController> _notes = new();

        public GameStageNotesManager(GameStageContext stage)
        {
            _stage = stage;
            _context = stage.NotesContext;

            _stage.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.HighlightedNoteSpeed)) || e.MatchProperty(nameof(s.IsFilterNoteSpeed))) {
                    foreach (var note in _notes) {
                        note.RefreshHighlightState();
                    }
                }
                if (e.MatchProperty(nameof(s.IsShowLinkLines))) {
                    foreach (var note in _notes) {
                        note.RefreshLinkLine();
                    }
                }
                if (e.MatchProperty(nameof(s.IsDistinguishPianoNotes))) {
                    foreach (var note in _notes) {
                        note.RefreshVisual();
                    }
                }
                if (e.MatchProperty(nameof(s.SuddenPlus)) || e.MatchProperty(nameof(s.IsEarlyDisplaySlowNotes))) {
                    // REFACTOR: 我不知道为什么SuddenPlus改变时这里会全刷新一次后又单个刷新，感觉写错了
                    //NotesManager.RefreshStageActiveNotes();
                    foreach (var note in _notes) {
                        note.RefreshStageDeltaTime();
                    }
                }
            });

            _context.ActiveNotesChanged += (s, e) =>
            {
                foreach (var note in _notes) {
                    _factory.Return(note);
                }
                _notes.Clear();
                foreach (var note in s.ActiveNotes) {
                    var controller = _factory.Create();
                    controller.Initialize(note);
                    _notes.Add(controller);
                }
                ModelAsserts.AssertInOrderViaTimeUnique(s.ActiveNotes);
                GameStageNoteController? prevNote = null;
                foreach(var note in _notes) {
                    note.PostInitialize(prevNote);
                    prevNote = note;
                }
            };
        }

        public void Initialize(IGameStageNoteFactory noteFactory)
        {
            _factory = noteFactory;
            foreach (var note in _context.ActiveNotes) {
                var controller = _factory.Create();
                controller.Initialize(note);
                _notes.Add(controller);
            }
        }
    }
}
