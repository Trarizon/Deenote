#nullable enable

using Deenote.Contexts;
using Deenote.CoreB.Notification;
using Deenote.GameStage.Themes;
using System;
using UnityEngine;

namespace Deenote.GameStage
{
    internal sealed class GameStageManager
    {
        private readonly GameStageContext _context;
        private readonly ProjectContext _project;
        private readonly GameStageNotesManager _notesManager;

        public GameStageManager(GameStageContext context, ProjectContext project,GameStageThemeManager themeManager)
        {
            _context = context;
            _project = project;
            _notesManager = new(context);

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

                    } catch (Exception ex) {
                        Debug.LogError(ex.Message + ex.StackTrace);
                        throw;
                    }
                }
            });
        }
    }
}
