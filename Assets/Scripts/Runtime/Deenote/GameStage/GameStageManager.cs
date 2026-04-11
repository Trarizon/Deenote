#nullable enable

using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using System;
using UnityEngine;

namespace Deenote.GameStage
{
    internal sealed class GameStageManager
    {
        private readonly GameStageContext _context;
        private readonly GameStageNotesManager _notesManager;
        private readonly GameStageGridsManager _gridsManager;

        public GameStageManager(GameStageContext context, EditorContext editor, GamePlayContext gamePlay, GameStageThemeManager themeManager)
        {
            _context = context;
            _notesManager = new(context);
            _gridsManager = new(editor.Grids, gamePlay, context);

            _context.ThemeContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentTheme))) {
                    if (s.CurrentTheme is null)
                        return;
                    if (_context.GameStage is null)
                        return;
                    try {
                        _context.GameStage.Initialize(_context);
                        _notesManager.Initialize(s.CurrentTheme.NoteFactory);
                        _context.NotesContext.RefreshActiveVisibleNotes();
                        _gridsManager.SetLinesRenderer(_context.GameStage.PerspectiveLinesRenderer);
                    } catch (Exception ex) {
                        Debug.LogError(ex.Message + ex.StackTrace);
                        throw;
                    }
                }
            });
        }
    }
}
